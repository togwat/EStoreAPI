import { useState, useEffect } from 'react';

const STORAGE_KEY = 'autoOpenReasoning';
const CHANGE_EVENT = 'auto-open-reasoning-change';

const DEFAULT_AUTO_OPEN_REASONING = true;

export function setAutoOpenReasoning(value: boolean): void {
    try { 
        localStorage.setItem(STORAGE_KEY, String(value)); } catch { /* storage unavailable */ }
    // emit event for useAutoOpenReasoning hook
    window.dispatchEvent(new CustomEvent(CHANGE_EVENT, { detail: value }));
}

export function getAutoOpenReasoning(): boolean {
    try {
        const saved = localStorage.getItem(STORAGE_KEY);
        if (saved === 'true' || saved === 'false') return saved === 'true';
    } catch { /* storage unavailable */ }
    return DEFAULT_AUTO_OPEN_REASONING;
}

// lets already-mounted chat messages pick up the change without a reload
export function useAutoOpenReasoning(): boolean {
    const [value, setValue] = useState(getAutoOpenReasoning);
    useEffect(() => {
        function handleChange(e: Event) {
            setValue((e as CustomEvent<boolean>).detail);
        }
        window.addEventListener(CHANGE_EVENT, handleChange);
        return () => window.removeEventListener(CHANGE_EVENT, handleChange);
    }, []);
    return value;
}
