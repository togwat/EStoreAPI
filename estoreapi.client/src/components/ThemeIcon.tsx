import { getTheme } from '@/lib/theme';
import { themeIcons } from '@/lib/themeLogos';

export function ThemeIcon({ alt = '', ...props }: React.ImgHTMLAttributes<HTMLImageElement>) {
    return <img src={themeIcons[getTheme()]} alt={alt} {...props} />;
}
