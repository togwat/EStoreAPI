/**
 * Chat history persistence API — thin fetch wrappers over the agent's `/agent/store`
 * routes (proxied via `^/agent`, same as the chat stream in Chat.tsx). These back
 * assistant-ui's RemoteThreadListAdapter (thread list) and ThreadHistoryAdapter
 * (per-thread messages); identity is supplied by the X-User-Email header injected
 * upstream, so no auth wiring is needed here.
 */
import type {
	ExportedMessageRepository,
	ExportedMessageRepositoryItem,
	ThreadMessage,
} from "@assistant-ui/react";

const BASE = "/agent/store";

/** Session metadata as returned by the backend. */
export type ChatSummary = { remoteId: string; title: string | null };

async function request<T>(url: string, init?: RequestInit): Promise<T> {
	const res = await fetch(url, {
		headers: { "Content-Type": "application/json" },
		...init,
	});
	if (!res.ok) throw new Error(`Chat store error ${res.status}: ${res.statusText}`);
	// 204 No Content (patch/delete/append) — nothing to parse.
	if (res.status === 204) return undefined as T;
	return (await res.json()) as T;
}

/** Adapter `list`: all of the user's sessions, newest first. */
export const listChats = () =>
	request<{ threads: ChatSummary[] }>(BASE);

/** Adapter `initialize`: create a new session server-side and return its remoteId. */
export const createChat = () =>
	request<{ remoteId: string; externalId: string | null }>(BASE, {
		method: "POST",
	});

/** Adapter `fetch`: one session's metadata. */
export const getChat = (id: string) =>
	request<ChatSummary>(`${BASE}/${id}`);

/** Adapter `rename`: update a session's title. */
export const patchChat = (id: string, title: string) =>
	request<void>(`${BASE}/${id}`, {
		method: "PATCH",
		body: JSON.stringify({ title }),
	});

/** Adapter `delete`: remove a session and its messages. */
export const deleteChat = (id: string) =>
	request<void>(`${BASE}/${id}`, { method: "DELETE" });

/** History `load`: the session's full message repository for restore. */
export const getMessages = (id: string) =>
	request<ExportedMessageRepository>(`${BASE}/${id}/messages`);

/** History `append`: persist one finalized message. */
export const appendMessage = (id: string, item: ExportedMessageRepositoryItem) =>
	request<void>(`${BASE}/${id}/messages`, {
		method: "POST",
		body: JSON.stringify(item),
	});

/** The first user message's text, used as the basis for a title. */
function firstUserText(messages: readonly ThreadMessage[]): string {
	const message = messages.find((m) => m.role === "user");
	const part = message?.content.find((p) => p.type === "text");
	return part?.type === "text" ? part.text.trim() : "";
}

/**
 * Ask the LLM for a short conversation title, reusing the existing `/agent/chat`
 * endpoint (no title-specific endpoint). Only the opening user message is sent
 */
export async function generateTitle(messages: readonly ThreadMessage[]): Promise<string> {
	const opening = firstUserText(messages);
	const prompt =
		"Generate a concise 3-6 word title for a conversation that begins with the " +
		`message below. Reply with only the title, no quotes or trailing punctuation.\n\n"${opening}"`;

	const res = await fetch("/agent/chat", {
		method: "POST",
		headers: { "Content-Type": "application/json" },
		body: JSON.stringify({
			messages: [{ role: "user", content: [{ type: "text", text: prompt }] }],
			stream: true,
		}),
	});
	if (!res.ok || !res.body) throw new Error(`Title generation failed: ${res.status}`);

	// The endpoint streams NDJSON events; accumulate the ["chunk", text] deltas.
	const reader = res.body.getReader();
	const decoder = new TextDecoder();
	let buffer = "";
	let title = "";
	for (;;) {
		const { done, value } = await reader.read();
		if (done) break;
		buffer += decoder.decode(value, { stream: true });
		const lines = buffer.split("\n");
		buffer = lines.pop() ?? "";
		for (const line of lines) {
			if (!line.trim()) continue;
			try {
				const event = JSON.parse(line) as [string, ...unknown[]];
				if (event[0] === "chunk") title += event[1] as string;
			} catch {
				// ignore non-JSON / partial lines
			}
		}
	}

	return title.trim().replace(/^["']|["']$/g, "").slice(0, 80);
}

/** Fallback in case LLM-generated title is unavailable
 * Truncated first message. */
export function fallbackTitle(messages: readonly ThreadMessage[]): string {
	const text = firstUserText(messages);
	if (!text) return "New chat";
	return text.length > 50 ? `${text.slice(0, 47)}...` : text;
}