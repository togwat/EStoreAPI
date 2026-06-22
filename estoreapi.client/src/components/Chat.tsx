import type { FC, PropsWithChildren } from "react";
import { useEffect, useMemo } from "react";
import {
    AssistantRuntimeProvider,
    useLocalRuntime,
    useRemoteThreadListRuntime,
    RuntimeAdapterProvider,
    useAui,
    type ChatModelAdapter,
    type RemoteThreadListAdapter,
    type ThreadHistoryAdapter,
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
import { AssistantSidebar } from "./assistant-ui/assistant-sidebar";

/**
 * A single tool call emitted by the backend, with an optional result once execution completes.
 * `result` being absent means the tool is still running; assistant-ui renders a spinner until it appears.
 */
type ToolCallPart = {
    type: "tool-call";
    toolCallId: string;
    toolName: string;
    /** Pretty-printed JSON string of the tool's arguments, consumed by ToolFallback for display. */
    argsText: string;
    result?: unknown;
};

/** for sources component of web page searches */
type SourcePart = {
    type: "source";
    sourceType: "url";
    url: string; 
    title?: string;
};

/** Content part shapes received from the agent and yielded to the assistant-ui runtime. */
type IncomingContentPart =
    | { type: "reasoning"; text: string }
    | { type: "text"; text: string }
    | ToolCallPart
    | SourcePart;

/** Content part shapes sent to the agent over the wire. */
type OutgoingContentPart =
    | { type: "text"; text: string }
    | { type: "image_url"; url: string }
    | { type: "reasoning"; text: string }
    | { type: "tool_use"; id: string; name: string; input: unknown; result?: unknown };

/** Streaming event shapes from /agent/chat's StreamingResponse */
type StreamEvent =
    | ["chunk", string]
    | ["reasoning", string]
    | ["tool_calls", Array<{ id: string; name: string; arguments: Record<string, unknown> }>]
    | ["tool_result", string, unknown]
    // The listed tool calls need user approval; the backend ends the stream here.
    | ["confirmation_required", string[]];

/** Convert a single assistant-ui content part into the agent's wire shape. */
const toOutgoingParts = (c: { type: string; [k: string]: unknown }): OutgoingContentPart[] => {
    if (c.type === "text") return [{ type: "text", text: c.text as string }];
    if (c.type === "image") return [{ type: "image_url", url: c.image as string }];
    // Round-tripped so thinking mode can re-receive the reasoning behind a tool call. Only works for API providers
    if (c.type === "reasoning") return [{ type: "reasoning", text: c.text as string }];
    if (c.type === "tool-call") return [{
        type: "tool_use",
        id: c.toolCallId as string,
        name: c.toolName as string,
        input: JSON.parse(c.argsText as string),
        result: (c as ToolCallPart).result,
    }];
    return [];
};

const agentAdapter: ChatModelAdapter = {
    async *run({ messages, abortSignal, unstable_getMessage }) {
        const formatted = messages.map(m => ({
            role: m.role,
            content: [
                ...m.content.flatMap(c => toOutgoingParts(c as never)),
                ...(m.role === "user"
                    ? m.attachments.flatMap(a => (a.content ?? []).flatMap(c => toOutgoingParts(c as never)))
                    : []),
            ],
        }));

        // On a roundtrip after a confirmation, the runtime passes history only up
        // to the user message. The in-progress assistant message holds the
        // confirmed tool call and its result, so append it for the backend.
        const current = unstable_getMessage();
        if (current.content.some(c => c.type === "tool-call")) {
            formatted.push({
                role: "assistant",
                content: current.content.flatMap(c => toOutgoingParts(c as never)),
            });
        }

        const res = await fetch("/agent/chat", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ messages: formatted, stream: true }),
            signal: abortSignal,
        });

        if (!res.ok) throw new Error(`Agent error: ${res.statusText}`);
        if (!res.body) throw new Error("No response body");

        // accumulated state across the stream
        let text = "";
        let reasoning = "";
        const toolCalls: ToolCallPart[] = [];
        const toolCallMap = new Map<string, ToolCallPart>();
        const sources: SourcePart[] = [];
        // Set when the backend pauses for confirmation; drives the final status.
        let awaitingConfirmation = false;

        /** Assemble the current accumulated state into an ordered content array. */
        const buildContent = (): IncomingContentPart[] => [
            ...(reasoning ? [{ type: "reasoning" as const, text: reasoning }] : []),
            ...toolCalls,
            ...(text ? [{ type: "text" as const, text }] : []),
            ...sources,
        ];

        /** Apply one parsed event to the accumulated state. Returns true if state changed. */
        const applyEvent = ([type, ...rest]: StreamEvent): boolean => {
            if (type === "chunk") {
                text += rest[0] as string;
            } else if (type === "reasoning") {
                reasoning += rest[0] as string;
            } else if (type === "tool_calls") {
                for (const call of rest[0] as StreamEvent[1] & object[]) {
                    const part: ToolCallPart = {
                        type: "tool-call",
                        toolCallId: call.id,
                        toolName: call.name,
                        argsText: JSON.stringify(call.arguments, null, 2),
                    };
                    toolCalls.push(part);
                    toolCallMap.set(call.id, part);
                }
            } else if (type === "tool_result") {
                const [id, result] = rest as [string, unknown];
                const part = toolCallMap.get(id);
                if (part) {
                    try {
                        // split web search tool call results into text results (go to the agent)
                        // and the sources (render badges)
                        const parsed = JSON.parse(result as string) as { text?: string; sources?: { url: string; title?: string }[] };
                        // Fall back to the raw string if the result isn't in the {text, sources} web-search shape,
                        // which are usually from regular tool calls without a 'text' field
                        part.result = parsed.text ?? result;
                        for (const s of parsed.sources ?? []) {
                            sources.push({ type: "source", sourceType: "url", url: s.url, title: s.title });
                        }
                    } catch {
                        // Plain-string tool results are not JSON, assign directly.
                        part.result = result;
                    }
                }
            } else if (type === "confirmation_required") {
                // Tool call(s) await user approval; the stream ends after this.
                awaitingConfirmation = true;
            }
            return true;
        };

        /** Parse one NDJSON line and apply it. Returns true if a state change occurred. */
        const processLine = (line: string): boolean => {
            const trimmed = line.trim();
            if (!trimmed) return false;
            try {
                return applyEvent(JSON.parse(trimmed) as StreamEvent);
            } catch {
                return false;
            }
        };

        // read the stream line by line
        const reader = res.body.getReader();
        const decoder = new TextDecoder();
        let buffer = "";

        while (true) {
            const { done, value } = await reader.read();
            if (done) break;

            buffer += decoder.decode(value, { stream: true });
            // Split on newlines; keep the last (potentially incomplete) chunk in the buffer.
            const lines = buffer.split("\n");
            buffer = lines.pop() ?? "";
            let changed = false;
            for (const line of lines) {
                if (processLine(line)) changed = true;
            }
            if (changed) yield { content: buildContent() as never[] };
        }

        // Flush any remaining bytes that arrived without a trailing newline.
        if (buffer.trim()) processLine(buffer);

        // Final emit. When the backend paused for confirmation, mark the message
        // `requires-action` so the runtime stops and waits — the user's decision
        // arrives later via addToolResult, which resumes the run. Otherwise the
        // message completes normally.
        yield {
            content: buildContent() as never[],
            ...(awaitingConfirmation
                ? { status: { type: "requires-action", reason: "tool-calls" } as const }
                : {}),
        };
    },
};

const attachmentAdapter = new CompositeAttachmentAdapter([
    new SimpleImageAttachmentAdapter(),
    new MarkdownAttachmentAdapter(),
    new SimpleTextAttachmentAdapter(),
]);

/**
 * Per-thread history adapter, injected into each thread's runtime via context.
 * Mirrors assistant-ui's cloud adapter: the remoteId is read from the active thread
 * list item, and `initialize()` is awaited before the first write so a brand-new
 * thread is created server-side exactly once.
 */
const HistoryProvider: FC<PropsWithChildren> = ({ children }) => {
    const aui = useAui();
    const history = useMemo<ThreadHistoryAdapter>(
        () => ({
            async load() {
                const remoteId = aui.threadListItem().getState().remoteId;
                if (!remoteId) return { messages: [] };
                return getMessages(remoteId);
            },
            async append(item) {
                const { remoteId } = await aui.threadListItem().initialize();
                await appendMessage(remoteId, item);
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
const remoteThreadListAdapter: RemoteThreadListAdapter = {
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
    async initialize(threadId) {
        const { remoteId, externalId } = await createChat(threadId);
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

// localStorage key holding the last-open thread's remoteId, so a reload or agent restart reopens the same chat.
const ACTIVE_THREAD_KEY = "estore-active-chat";

export default function Chat({ children }: PropsWithChildren) {
    // Restore the last-open thread on mount (constant, so it only seeds the initial
    // thread and doesn't fight the native thread switching used by the header).
    const initialThreadId = useMemo(
        () => localStorage.getItem(ACTIVE_THREAD_KEY) ?? undefined,
        [],
    );

    const runtime = useRemoteThreadListRuntime({
        runtimeHook: () =>
            useLocalRuntime(agentAdapter, { adapters: { attachments: attachmentAdapter } }),
        adapter: remoteThreadListAdapter,
        threadId: initialThreadId,
    });

    // Persist the active thread's remoteId whenever it changes, so reload reopens it.
    useEffect(() => {
        const threads = runtime.threads;
        const persist = () => {
            try {
                const remoteId = threads.getItemById(threads.getState().mainThreadId).getState().remoteId;
                if (remoteId) localStorage.setItem(ACTIVE_THREAD_KEY, remoteId);
            } catch {
                // no initialized remote thread yet, nothing to persist
            }
        };
        persist();
        return threads.subscribe(persist);
    }, [runtime]);

    return (
        <AssistantRuntimeProvider runtime={runtime}>
            <AssistantSidebar>
                {children}
            </AssistantSidebar>
        </AssistantRuntimeProvider>
    );
}
