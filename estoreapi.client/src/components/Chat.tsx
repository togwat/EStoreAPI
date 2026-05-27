import type { PropsWithChildren } from "react";
import {
    AssistantRuntimeProvider,
    useLocalRuntime,
    type ChatModelAdapter,
    SimpleImageAttachmentAdapter,
    SimpleTextAttachmentAdapter,
    CompositeAttachmentAdapter,
} from "@assistant-ui/react";
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

/** Union of all content part shapes yielded to the assistant-ui runtime. */
type ContentPart =
    | { type: "reasoning"; text: string }
    | { type: "text"; text: string }
    | ToolCallPart;

/** Streaming event shapes from /agent/chat's StreamingResponse */
type StreamEvent =
    | ["chunk", string]
    | ["reasoning", string]
    | ["tool_calls", Array<{ id: string; name: string; arguments: Record<string, unknown> }>]
    | ["tool_result", string, unknown];

const agentAdapter: ChatModelAdapter = {
    async *run({ messages, abortSignal }) {
        const formatted = messages.map(m => ({
            role: m.role,
            content: m.content
                .filter(c => c.type === "text")
                .map(c => (c as { type: "text"; text: string }).text)
                .join(""),
        }));

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

        /** Assemble the current accumulated state into an ordered content array. */
        const buildContent = (): ContentPart[] => [
            ...(reasoning ? [{ type: "reasoning" as const, text: reasoning }] : []),
            ...toolCalls,
            ...(text ? [{ type: "text" as const, text }] : []),
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
                if (part) part.result = result;
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
        if (buffer.trim() && processLine(buffer)) {
            yield { content: buildContent() as never[] };
        }
    },
};

const attachmentAdapter = new CompositeAttachmentAdapter([
    new SimpleImageAttachmentAdapter(),
    new SimpleTextAttachmentAdapter(),
]);

export default function Chat({ children }: PropsWithChildren) {
    const runtime = useLocalRuntime(agentAdapter, {
        adapters: { attachments: attachmentAdapter },
    });
    return (
        <AssistantRuntimeProvider runtime={runtime}>
            <AssistantSidebar>
                {children}
            </AssistantSidebar>
        </AssistantRuntimeProvider>
    );
}
