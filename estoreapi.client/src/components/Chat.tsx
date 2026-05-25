import type { PropsWithChildren } from "react";
import { AssistantRuntimeProvider, useLocalRuntime, type ChatModelAdapter } from "@assistant-ui/react";
import { AssistantSidebar } from "./assistant-ui/assistant-sidebar";

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

        const reader = res.body.getReader();
        const decoder = new TextDecoder();
        let text = "";

        while (true) {
            const { done, value } = await reader.read();
            if (done) break;
            text += decoder.decode(value, { stream: true });
            yield { content: [{ type: "text" as const, text }] };
        }
    },
};

export default function Chat({ children }: PropsWithChildren) {
    const runtime = useLocalRuntime(agentAdapter);
    return (
        <AssistantRuntimeProvider runtime={runtime}>
            <AssistantSidebar>
                {children}
            </AssistantSidebar>
        </AssistantRuntimeProvider>
    );
}