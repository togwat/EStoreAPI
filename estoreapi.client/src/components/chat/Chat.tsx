import type { PropsWithChildren } from "react";
import { useEffect, useMemo } from "react";
import {
    AssistantRuntimeProvider,
    useLocalRuntime,
    useRemoteThreadListRuntime,
} from "@assistant-ui/react";
import { agentAdapter } from "./agentStream";
import { attachmentAdapter, remoteThreadListAdapter } from "./chatPersistence";
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
