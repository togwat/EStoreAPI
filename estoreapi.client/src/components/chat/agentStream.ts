import type { ChatModelAdapter, ThreadMessage } from "@assistant-ui/react";
import type { ThreadTokenUsage } from "@assistant-ui/react-ai-sdk";

/**
 * Wire protocol and stream handling for the `/agent/chat` endpoint.
 *
 * Responsibilities are split so each reads on its own:
 *  - wire types describe what crosses the network in each direction;
 *  - `toOutgoingParts` / `formatOutgoingMessages` assemble the request;
 *  - `TurnAccumulator` collects stream events into renderable content;
 *  - `readNdjsonLines` turns the response body into parsed events;
 *  - `agentAdapter` wires these together for assistant-ui's runtime.
 */

// --- wire types ------------------------------------------------------------

/**
 * A single tool call emitted by the backend, with an optional result once execution completes.
 * `result` being absent means the tool is still running; assistant-ui renders a spinner until it appears.
 */
export type ToolCallPart = {
    type: "tool-call";
    toolCallId: string;
    toolName: string;
    /** Pretty-printed JSON string of the tool's arguments, consumed by ToolFallback for display. */
    argsText: string;
    result?: unknown;
};

/** for sources component of web page searches */
export type SourcePart = {
    type: "source";
    sourceType: "url";
    id: string;
    url: string;
    title?: string;
};

/** Content part shapes received from the agent and yielded to the assistant-ui runtime. */
export type IncomingContentPart =
    | { type: "reasoning"; text: string }
    | { type: "text"; text: string }
    | ToolCallPart
    | SourcePart;

/** Content part shapes sent to the agent over the wire. */
export type OutgoingContentPart =
    | { type: "text"; text: string }
    | { type: "image_url"; url: string }
    | { type: "reasoning"; text: string }
    | { type: "tool_use"; id: string; name: string; input: unknown; result?: unknown };

/** Streaming event shapes from /agent/chat's StreamingResponse */
export type StreamEvent =
    | ["chunk", string]
    | ["reasoning", string]
    | ["tool_calls", Array<{ id: string; name: string; arguments: Record<string, unknown> }>]
    | ["tool_result", string, unknown]
    // Token counts for the latest model call; the frontend keeps the most recent.
    | ["usage", ThreadTokenUsage]
    // The listed tool calls need user approval; the backend ends the stream here.
    | ["confirmation_required", string[]]
    // An uncaught backend exception, surfaced with the response body because the HTTP status is already sent.
    | ["error", string];

// --- request assembly ------------------------------------------------------

/** Convert a single assistant-ui content part into the agent's wire shape. */
export function toOutgoingParts(c: { type: string; [k: string]: unknown }): OutgoingContentPart[] {
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
}

/**
 * Format the runtime's messages into the agent's wire shape.
 *
 * On a roundtrip after a confirmation, the runtime passes history only up
 * to the user message. The in-progress assistant message holds the
 * confirmed tool call and its result, so append it for the backend.
 */
type RunOptions = Parameters<ChatModelAdapter["run"]>[0];

function formatOutgoingMessages(messages: RunOptions["messages"], current: ThreadMessage) {
    const formatted = messages.map(m => ({
        role: m.role,
        content: [
            ...m.content.flatMap(c => toOutgoingParts(c as never)),
            ...(m.role === "user"
                ? m.attachments.flatMap(a => (a.content ?? []).flatMap(c => toOutgoingParts(c as never)))
                : []),
        ],
    }));

    if (current.content.some(c => c.type === "tool-call")) {
        formatted.push({
            role: "assistant",
            content: current.content.flatMap(c => toOutgoingParts(c as never)),
        });
    }

    return formatted;
}

/**
 * Split web search tool call results into text results (to agent) and sources (badges)
 * Any other tool result (plain string, other JSON) passes through with no sources.
 */
function splitWebSearchResult(raw: unknown, toolCallId: string): { result: unknown; sources: SourcePart[] } {
    try {
        const parsed = JSON.parse(raw as string) as { text?: string; sources?: { url: string; title?: string }[] };
        return {
            result: parsed.text ?? raw,
            sources: (parsed.sources ?? []).map((s, i) => ({
                type: "source", sourceType: "url", id: `${toolCallId}-${i}`, url: s.url, title: s.title,
            })),
        };
    } catch {
        return { result: raw, sources: [] };
    }
}

// --- response accumulation -------------------------------------------------

/**
 * Collects the parts of a single assistant turn as stream events arrive, then
 * assembles them into the ordered content array assistant-ui renders.
 */
class TurnAccumulator {
    private text = "";
    private reasoning = "";
    private readonly toolCalls: ToolCallPart[] = [];
    private readonly toolCallMap = new Map<string, ToolCallPart>();
    private readonly sources: SourcePart[] = [];
    // Latest token usage reported during the turn; surfaced as message metadata for the context indicator.
    usage: ThreadTokenUsage | undefined;
    // Set when the backend pauses for confirmation; drives the final status.
    awaitingConfirmation = false;
    // Set when the backend surfaces an exception, run() can throw it once the stream ends.
    error: string | undefined;

    /**
     * Render the web_search source badges if web_search is confirmation-gated
     * since results after confirmation does not trigger sources.push
     */
    seedSources(content: ThreadMessage["content"]): void {
        for (const c of content) {
            if (c.type === "tool-call" && c.toolName === "web_search" && c.result !== undefined) {
                this.sources.push(...splitWebSearchResult(c.result, c.toolCallId).sources);
            }
        }
    }

    /** Apply one parsed event to the accumulated state. */
    apply([type, ...rest]: StreamEvent): void {
        if (type === "chunk") {
            this.text += rest[0] as string;
        } else if (type === "reasoning") {
            this.reasoning += rest[0] as string;
        } else if (type === "tool_calls") {
            for (const call of rest[0] as Array<{ id: string; name: string; arguments: Record<string, unknown> }>) {
                const part: ToolCallPart = {
                    type: "tool-call",
                    toolCallId: call.id,
                    toolName: call.name,
                    argsText: JSON.stringify(call.arguments, null, 2),
                };
                this.toolCalls.push(part);
                this.toolCallMap.set(call.id, part);
            }
        } else if (type === "tool_result") {
            const [id, result] = rest as [string, unknown];
            const part = this.toolCallMap.get(id);
            if (part) {
                // try to extract web_search sources
                if (part.toolName === "web_search") {
                    const parsed = splitWebSearchResult(result, id);
                    part.result = parsed.result;
                    this.sources.push(...parsed.sources);
                } else {
                    part.result = result;
                }
            }
        } else if (type === "usage") {
            // Keep the most recent counts; the last model call reflects the fullest context.
            this.usage = rest[0] as ThreadTokenUsage;
        } else if (type === "confirmation_required") {
            // Tool call(s) await user approval; the stream ends after this.
            this.awaitingConfirmation = true;
        } else if (type === "error") {
            // Capture error message; run() throws it.
            this.error = rest[0] as string;
        }
    }

    /** Assemble the current accumulated state into an ordered content array. */
    build(): IncomingContentPart[] {
        return [
            ...(this.reasoning ? [{ type: "reasoning" as const, text: this.reasoning }] : []),
            ...this.toolCalls,
            ...(this.text ? [{ type: "text" as const, text: this.text }] : []),
            ...this.sources,
        ];
    }
}

/**
 * Read a response body as NDJSON, yielding the successfully-parsed events from
 * each network chunk. Lines are buffered across chunk boundaries; blank and
 * malformed lines are skipped, and a trailing line that arrived without a
 * newline is flushed at the end.
 */
async function* readNdjsonLines(body: ReadableStream<Uint8Array>): AsyncGenerator<StreamEvent[]> {
    const parse = (line: string): StreamEvent | undefined => {
        const trimmed = line.trim();
        if (!trimmed) return undefined;
        try {
            return JSON.parse(trimmed) as StreamEvent;
        } catch {
            return undefined;
        }
    };

    // read the stream line by line
    const reader = body.getReader();
    const decoder = new TextDecoder();
    let buffer = "";

    while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        buffer += decoder.decode(value, { stream: true });
        // Split on newlines; keep the last (potentially incomplete) chunk in the buffer.
        const lines = buffer.split("\n");
        buffer = lines.pop() ?? "";
        yield lines.map(parse).filter((e): e is StreamEvent => e !== undefined);
    }

    // Flush any remaining bytes that arrived without a trailing newline.
    const last = parse(buffer);
    if (last) yield [last];
}

// --- adapter ---------------------------------------------------------------

export const agentAdapter: ChatModelAdapter = {
    async *run({ messages, abortSignal, unstable_getMessage }) {
        const current = unstable_getMessage();
        const formatted = formatOutgoingMessages(messages, current);

        const res = await fetch("/agent/chat", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ messages: formatted, stream: true }),
            signal: abortSignal,
        });

        if (!res.ok) throw new Error(`Agent error: ${res.statusText}`);
        if (!res.body) throw new Error("No response body");

        const turn = new TurnAccumulator();
        turn.seedSources(current.content);

        // Token usage rides along as message metadata under `custom.usage`, where
        // useThreadTokenUsage() reads it to drive the context-usage indicator. It also
        // persists with the message, so the indicator survives a reload.
        const usageMeta = () =>
            turn.usage ? { metadata: { custom: { usage: turn.usage } } } : {};

        // Emit on every chunk that carried at least one event, so the UI streams.
        for await (const events of readNdjsonLines(res.body)) {
            if (events.length === 0) continue;
            for (const event of events) turn.apply(event);
            yield { content: turn.build() as never[], ...usageMeta() };
        }

        // Rethrowing the error will surface the message in the error box
        if (turn.error) throw new Error(turn.error);

        // Final emit. When the backend paused for confirmation, mark the message
        // `requires-action` so the runtime stops and waits. 
        // The user's decision arrives later via addToolResult, which resumes the 
        // run. Otherwise the message completes normally.
        yield {
            content: turn.build() as never[],
            ...usageMeta(),
            ...(turn.awaitingConfirmation
                ? { status: { type: "requires-action", reason: "tool-calls" } as const }
                : {}),
        };
    },
};
