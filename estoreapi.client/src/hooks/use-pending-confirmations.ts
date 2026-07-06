import { useAui, useAuiState } from "@assistant-ui/react";

type PendingConfirmation = { messageId: string; toolCallId: string };

/**
 * Finds tool calls that are awaiting user confirmation and lets the composer
 * cancel them in bulk.
 *
 * A gated tool call surfaces as a `requires-action` message with a tool-call
 * part that has no result yet (see ToolConfirmation / Chat.tsx's pause status).
 * This hook collects those and exposes `cancelAll`, which resolves each with a
 * reason string — used to treat "user sent a new message" as an implicit
 * decline so the run can continue.
 */
export function usePendingConfirmations() {
  const aui = useAui();
  const messages = useAuiState((s) => s.thread.messages);
  const thread = aui.thread();

  const pending: PendingConfirmation[] = [];
  for (const message of messages) {
    if (message.status?.type !== "requires-action") continue;
    for (const part of message.content) {
      if (part.type === "tool-call" && part.result === undefined) {
        pending.push({ messageId: message.id, toolCallId: part.toolCallId });
      }
    }
  }

  return {
    hasPending: pending.length > 0,
    /** Resolve every pending tool call with `reason`, declining them. */
    cancelAll: (reason: string) => {
      for (const { messageId, toolCallId } of pending) {
        thread
          .message({ id: messageId })
          .part({ toolCallId })
          .addToolResult(reason);
      }
    },
  };
}
