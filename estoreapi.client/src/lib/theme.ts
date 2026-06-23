import { useState, useEffect } from 'react';

export const themes = ['rustic-leather', 'generic-dark', 'generic-light', 'sakura-light'] as const;
export type ThemeName = typeof themes[number];

const DEFAULT_THEME: ThemeName = 'rustic-leather';

export function setTheme(name: ThemeName): void {
    document.documentElement.setAttribute('data-theme', name);
    try { localStorage.setItem('theme', name); } catch { /* storage unavailable */ }
    // emit event for useTheme hook
    window.dispatchEvent(new CustomEvent('theme-change', { detail: name }));
}

export function getTheme(): ThemeName {
    try {
        const saved = localStorage.getItem('theme') as ThemeName;
        if (themes.includes(saved)) return saved;
    } catch { /* storage unavailable */ }
    return DEFAULT_THEME;
}

export function initTheme(): void {
    setTheme(getTheme());
}

// for non-css theme-related components (such as themeIcon) to change live when swapping themes
export function useTheme(): ThemeName {
    const [theme, setThemeState] = useState<ThemeName>(getTheme);
    useEffect(() => {
        function handleChange(e: Event) {
            setThemeState((e as CustomEvent<ThemeName>).detail);
        }
        window.addEventListener('theme-change', handleChange);
        return () => window.removeEventListener('theme-change', handleChange);
    }, []);
    return theme;
}
