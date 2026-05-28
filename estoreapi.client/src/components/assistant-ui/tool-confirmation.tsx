"use client";

import { useState } from "react";
import { AlertCircleIcon, LoaderIcon } from "lucide-react";
import type { ToolCallMessagePartProps } from "@assistant-ui/react";
import { Button } from "src/components/ui/button";

/**
 * Inline approval UI for a tool the backend has gated.
 *
 * When the agent requests a tool that requires confirmation, the backend pauses
 * without running it and the runtime surfaces the call in a `requires-action`
 * state (see Chat.tsx / the confirmation registry). thread.tsx renders this
 * component for that pending call.
 *
 * Providing a result via `addResult` is what unblocks the run: assistant-ui then
 * does a roundtrip back to the agent.
 *   - Confirm → run the tool via POST /agent/tool and feed back the real result.
 *   - Cancel  → feed back a "declined" message so the model can acknowledge it.
 */
export function ToolConfirmation({
  toolName,
  args,
  argsText,
  addResult,
}: ToolCallMessagePartProps) {
  // Guards against a double-run while the /agent/tool request is in flight.
  const [busy, setBusy] = useState(false);

  const confirm = async () => {
    setBusy(true);
    try {
      const res = await fetch("/agent/tool", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          name: toolName,
          arguments: args ?? JSON.parse(argsText || "{}"),
        }),
      });
      if (!res.ok) throw new Error(res.statusText);
      const { result } = await res.json();
      addResult(result);
    } catch (e) {
      // Surface the failure as the tool result so the model (and user) see it.
      addResult(`Tool failed: ${String(e)}`);
      setBusy(false);
    }
  };

  const cancel = () => addResult("The user declined to run this tool.");

  return (
    <div className="aui-tool-confirmation-root w-full rounded-lg border border-amber-500/40 bg-amber-500/5 px-4 py-3 text-sm">
      <div className="flex items-center gap-2 font-medium">
        <AlertCircleIcon className="size-4 shrink-0 text-amber-500" />
        <span>
          Run tool <b>{toolName}</b>?
        </span>
      </div>

      {argsText && (
        <pre className="mt-2 overflow-x-auto whitespace-pre-wrap border-t pt-2 text-muted-foreground">
          {argsText}
        </pre>
      )}

      <div className="mt-3 flex justify-end gap-2">
        <Button variant="outline" size="sm" onClick={cancel} disabled={busy}>
          Cancel
        </Button>
        <Button size="sm" onClick={confirm} disabled={busy}>
          {busy && <LoaderIcon className="animate-spin" />}
          {busy ? "Running..." : "Confirm"}
        </Button>
      </div>
    </div>
  );
}
