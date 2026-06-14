export const themes = ['rustic-leather', 'generic-dark', 'generic-light'] as const;
export type ThemeName = typeof themes[number];

const DEFAULT_THEME: ThemeName = 'rustic-leather';

export function setTheme(name: ThemeName): void {
    document.documentElement.setAttribute('data-theme', name);
    try { localStorage.setItem('theme', name); } catch { /* storage unavailable */ }
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
