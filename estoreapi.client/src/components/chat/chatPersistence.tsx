import type { FC, PropsWithChildren } from "react";
import { useMemo } from "react";
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
 * Per-thread history adapter. Buffers a turn's user message until the replies
 * finalise, then persists the turn as an atomic unit
 */
class ChatHistoryAdapter implements ThreadHistoryAdapter {
    // Serialises writes so they never overlap
    private queue: Promise<unknown> = Promise.resolve();
    // The turn's user message, held until the reply decides commit or drop
    private pending: ExportedMessageRepositoryItem[] = [];

    constructor(private readonly aui: ReturnType<typeof useAui>) {}

    async load(): Promise<ExportedMessageRepository> {
        const remoteId = this.aui.threadListItem().getState().remoteId;
        if (!remoteId) return { messages: [] };
        // read-time inversion guard (see orderParentFirst)
        return orderParentFirst(await getMessages(remoteId));
    }

    append(item: ExportedMessageRepositoryItem): Promise<void> {
        const msg = item.message;
        // Buffer the user message
        if (msg.role !== "assistant") {
            this.pending.push(item);
            return Promise.resolve();
        }
        const status = msg.status;
        const errored = status?.type === "incomplete" && status.reason === "error";
        // Drop the whole turn (user message + error reply) if the run errored
        const batch = errored ? [] : [...this.pending, item];
        this.pending = [];
        return this.commit(batch);
    }

    // Persist a turn's messages in one batch
    // the user message (parent) is always stored before the children (replies)
    private commit(batch: ExportedMessageRepositoryItem[]): Promise<void> {
        // batch length 0 is from errors, so persist nothing
        if (batch.length === 0) return Promise.resolve();

        const run = this.queue.then(async () => {
            const { remoteId } = await this.aui.threadListItem().initialize();
            for (const it of batch) {
                await appendMessage(remoteId, { ...it, message: stripAttachmentFiles(it.message) });
            }
        });
        // Keep the chain alive on failure so one bad append can't wedge the queue.
        this.queue = run.catch(() => {});
        return run;
    }
}

/**
 * Injects a per-thread ChatHistoryAdapter into the active thread's runtime via context.
 * One instance per thread (memoized on the stable aui), so each thread keeps its own
 * write queue and pending buffer.
 */
const HistoryProvider: FC<PropsWithChildren> = ({ children }) => {
    const aui = useAui();
    const history = useMemo(() => new ChatHistoryAdapter(aui), [aui]);
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
