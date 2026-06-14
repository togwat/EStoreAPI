import EStoreRusticFull from '../assets/E-Store-rustic.svg';
import EStoreRusticIcon from '../assets/icon-rustic.svg';
import EStoreGDFull from '../assets/E-Store-gd.svg';
import EStoreGDIcon from '../assets/icon-gd.svg';
import EStoreGLFull from '../assets/E-Store-gl.svg';
import EStoreGLIcon from '../assets/icon-gl.svg';
import type { ThemeName } from './theme';

// Add an entry here whenever a new theme is introduced.
export const themeLogos: Record<ThemeName, string> = {
    'rustic-leather': EStoreRusticFull,
    'generic-dark': EStoreGDFull,
    'generic-light': EStoreGLFull,
};

export const themeIcons: Record<ThemeName, string> = {
    'rustic-leather': EStoreRusticIcon,
    'generic-dark': EStoreGDIcon,
    'generic-light': EStoreGLIcon,
}