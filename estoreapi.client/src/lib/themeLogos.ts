import EStoreRusticFull from '../assets/E-Store-rustic.svg';
import EStoreRusticIcon from '../assets/icon-rustic.svg';
import type { ThemeName } from './theme';

// Add an entry here whenever a new theme is introduced.
export const themeLogos: Record<ThemeName, string> = {
    'rustic-leather': EStoreRusticFull,
};

export const themeIcons: Record<ThemeName, string> = {
    'rustic-leather': EStoreRusticIcon,
}