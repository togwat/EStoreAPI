import { createContext, useContext, useEffect, useId } from "react";

/**
 * UI-awareness: lets any component tell the agent what the user currently has on screen.
 *
 * Each mounted caller of `useUiAwareness` contributes one line
 * Chat snapshots every line when a run starts and sends them as the request's `uiContext`.
 * Model-only, not persisted in chat history
 */

// A Map so lines keep their registration order, and updating a line keeps its position.

export type UiAwarenessRegistry = Map<string, string>;

export const UiAwarenessContext = createContext<UiAwarenessRegistry | null>(null);

// Report one line of UI context while this component is mounted
export function useUiAwareness(line: string | null) {
    const registry = useContext(UiAwarenessContext);
    const key = useId();

    // Set or clear this caller's line in place, so a changing line doesn't move to the end.
    useEffect(() => {
        if (!registry) return;
        if (line === null) registry.delete(key);
        else registry.set(key, line);
    }, [registry, key, line]);

    // Drop the line once the component unmounts (e.g. navigating away from the page).
    useEffect(() => {
        if (!registry) return;
        return () => { registry.delete(key); };
    }, [registry, key]);
}
