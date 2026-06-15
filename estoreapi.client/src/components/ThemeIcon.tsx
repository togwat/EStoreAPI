import { useTheme } from '@/lib/theme';
import { themeIcons, themeLogos } from '@/lib/themeLogos';

export function ThemeIcon({ alt = '', ...props }: React.ImgHTMLAttributes<HTMLImageElement>) {
    return <img src={themeIcons[useTheme()]} alt={alt} {...props} />;
}

export function ThemeLogo({ alt = '', ...props }: React.ImgHTMLAttributes<HTMLImageElement>) {
    return <img src={themeLogos[useTheme()]} alt={alt} {...props} />;
}