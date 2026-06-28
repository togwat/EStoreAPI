import type { FC, PropsWithChildren } from "react";
import { useMemo, useRef } from "react";
import {
    RuntimeAdapterProvider,
    useAui,
    type RemoteThreadListAdapter,
    type ThreadHistoryAdapter,
    type ThreadMessage,
    type ExportedMessageRepository,
    type ExportedMessageRepositoryItem,
    SimpleImageAttachmentAdapter,
    SimpleTextAttachmentAdapter,
    CompositeAttachmentAdapter,
} from "@assistant-ui/react";
import { createAssistantStream } from "assistant-stream";
import MarkdownAttachmentAdapter from "@/adapters/MarkdownAttachmentAdapter";
import {
    listChats,
    createChat,
    getChat,
    patchChat,
    deleteChat,
    getMessages,
    appendMessage,
    generateTitle,
    fallbackTitle,
} from "@/api/store";

/**
 * Chat persistence wiring for assistant-ui: how attachments are decoded, how a
 * thread's message history is loaded/saved, and how the thread list maps to the
 * agent's `/agent/store` routes.
 */

export const attachmentAdapter = new CompositeAttachmentAdapter([
    new SimpleImageAttachmentAdapter(),
    new MarkdownAttachmentAdapter(),
    new SimpleTextAttachmentAdapter(),
]);

/**
 * A File can't survive JSON persistence as it round-trips into a `{}` that crashes
 * `URL.createObjectURL`. Drop it so a restored image attachment renders from its
 * data-URL content instead.
 */
export function stripAttachmentFiles(message: ThreadMessage): ThreadMessage {
    if (!("attachments" in message) || !message.attachments?.length) return message;
    return {
        ...message,
        attachments: message.attachments.map((a) => {
            const copy = { ...a };
            delete (copy as { file?: unknown }).file;
            return copy;
        }),
    } as ThreadMessage;
}

/**
 * Load time inversion guard: 
 * Reorder a stored repository's messages so every message comes after its parent. 
 * append() now serialises writes, so new sessions are already parent-first. This is an extra guard
 */
function orderParentFirst(repo: ExportedMessageRepository): ExportedMessageRepository {
    const ids = new Set(repo.messages.map((m) => m.message.id));
    const children = new Map<string, ExportedMessageRepositoryItem[]>();
    const roots: ExportedMessageRepositoryItem[] = [];
    for (const item of repo.messages) {
        // A message whose parent isn't in this session is treated as a root, so one broken
        // link can't strand everything after it.
        if (item.parentId == null || !ids.has(item.parentId)) {
            roots.push(item);
        } else {
            const bucket = children.get(item.parentId);
            if (bucket) bucket.push(item);
            else children.set(item.parentId, [item]);
        }
    }

    const ordered: ExportedMessageRepositoryItem[] = [];
    const stack = [...roots].reverse();
    while (stack.length > 0) {
        const node = stack.pop()!;
        ordered.push(node);
        const kids = children.get(node.message.id);
        if (kids) for (let i = kids.length - 1; i >= 0; i--) stack.push(kids[i]);
    }
    return { ...repo, messages: ordered };
}

/**
 * Per-thread history adapter, injected into each thread's runtime via context.
 * Mirrors assistant-ui's cloud adapter: the remoteId is read from the active thread
 * list item, and `initialize()` is awaited before the first write so a brand-new
 * thread is created server-side exactly once.
 */
const HistoryProvider: FC<PropsWithChildren> = ({ children }) => {
    const aui = useAui();
    // mutable queue survive across renders
    const queueRef = useRef<Promise<unknown>>(Promise.resolve());
    const history = useMemo<ThreadHistoryAdapter>(
        () => ({
            async load() {
                const remoteId = aui.threadListItem().getState().remoteId;
                if (!remoteId) return { messages: [] };
                // load messages with read time inversion guard
                return orderParentFirst(await getMessages(remoteId));
            },
            // Write time inversion guard:
            // useLocalRuntime fires the user-message append without awaiting it before starting 
            // the run, so on a slow turn its POST can land after the assistant reply's and invert
            // their stored order (child before parent), which breaks load. 
            // Chaining each append onto the previous one persists them in call order when writing
            // to ChatStore db.
            append(item) {
                const run = queueRef.current.then(async () => {
                    const { remoteId } = await aui.threadListItem().initialize();
                    await appendMessage(remoteId, { ...item, message: stripAttachmentFiles(item.message) });
                });
                // Keep the chain alive on failure so one bad append can't wedge the queue.
                queueRef.current = run.catch(() => {});
                return run;
            },
        }),
        [aui],
    );
    return (
        <RuntimeAdapterProvider adapters={{ history }}>
            {children}
        </RuntimeAdapterProvider>
    );
};

/**
 * Thread-list persistence backed by the agent's /agent/store endpoints.
 * Each method maps 1:1 to a backend route.
 * Titles are generated by the LLM on first turn (falling back to a truncated message) and persisted via `rename`.
 */
export const remoteThreadListAdapter: RemoteThreadListAdapter = {
    async list() {
        const { threads } = await listChats();
        return {
            // status is always "regular" (the backend has no archive concept)
            threads: threads.map((t) => ({
                status: "regular",
                remoteId: t.remoteId,
                title: t.title ?? undefined,
            })),
        };
    },
    async initialize() {
        const { remoteId, externalId } = await createChat();
        return { remoteId, externalId: externalId ?? undefined };
    },
    async rename(remoteId, newTitle) {
        await patchChat(remoteId, newTitle);
    },
    // archive isn't surfaced in the UI; satisfy the interface as no-ops.
    async archive() {},
    async unarchive() {},
    async delete(remoteId) {
        await deleteChat(remoteId);
    },
    async fetch(threadId) {
        const t = await getChat(threadId);
        return { status: "regular", remoteId: t.remoteId, title: t.title ?? undefined };
    },
    async generateTitle(remoteId, messages) {
        let title: string;
        try {
            title = (await generateTitle(messages));
        } catch {
            // llm title not available for whatever reason
            title = fallbackTitle(messages);
        }
        await patchChat(remoteId, title);
        return createAssistantStream((controller) => {
            controller.appendText(title);
        });
    },
    unstable_Provider: HistoryProvider,
};
