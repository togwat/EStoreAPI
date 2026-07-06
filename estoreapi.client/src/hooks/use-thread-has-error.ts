import { useThread } from "@assistant-ui/react";

/**
 * True when the thread's most recent message ended in an error.
 */
export function useThreadHasError() {
  return useThread((t) => {
    const status = t.messages.at(-1)?.status;
    return status?.type === "incomplete" && status.reason === "error";
  });
}
