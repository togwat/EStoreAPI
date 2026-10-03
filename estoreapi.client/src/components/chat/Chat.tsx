import type { PropsWithChildren } from "react";
import { useEffect, useMemo } from "react";
import {
    AssistantRuntimeProvider,
    useLocalRuntime,
    useRemoteThreadListRuntime,
} from "@assistant-ui/react";
import { createAgentAdapter } from "./agentStream";
import { attachmentAdapter, remoteThreadListAdapter } from "./chatPersistence";
import { UiAwarenessContext, type UiAwarenessRegistry } from "./UiAwareness";
import { AssistantSidebar } from "../assistant-ui/assistant-sidebar";

// localStorage key holding the last-open thread's remoteId, so a reload or agent restart reopens the same chat.
const ACTIVE_THREAD_KEY = "estore-active-chat";

export default function Chat({ children }: PropsWithChildren) {
    // Restore the last-open thread on mount (constant, so it only seeds the initial
    // thread and doesn't fight the native thread switching used by the header).
    const initialThreadId = useMemo(
        () => localStorage.getItem(ACTIVE_THREAD_KEY) ?? undefined,
        [],
    );

    // UI-awareness lines reported by the pages below (see UiAwareness.tsx); the adapter
    // snapshots them when a run starts. Both are created once so the runtime keeps a stable adapter.
    const uiAwareness = useMemo<UiAwarenessRegistry>(() => new Map(), []);
    const agentAdapter = useMemo(
        () => createAgentAdapter(() => [...uiAwareness.values()]),
        [uiAwareness],
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
        <UiAwarenessContext.Provider value={uiAwareness}>
            <AssistantRuntimeProvider runtime={runtime}>
                <AssistantSidebar>
                    {children}
                </AssistantSidebar>
            </AssistantRuntimeProvider>
        </UiAwarenessContext.Provider>
    );
}
