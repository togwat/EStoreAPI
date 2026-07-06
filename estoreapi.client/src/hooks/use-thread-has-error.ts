import { useAuiState } from "@assistant-ui/react";

/**
 * True when the thread's most recent message ended in an error.
 */
export function useThreadHasError() {
  return useAuiState((s) => {
    const last = s.thread.messages.at(-1);
    return (
      last?.role === "assistant" &&
      last.status.type === "incomplete" &&
      last.status.reason === "error"
    );
  });
}
