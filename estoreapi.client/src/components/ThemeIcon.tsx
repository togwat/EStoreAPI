import { useTheme } from '@/lib/theme';
import { themeIcons } from '@/lib/themeLogos';

export function ThemeIcon({ alt = '', ...props }: React.ImgHTMLAttributes<HTMLImageElement>) {
    return <img src={themeIcons[useTheme()]} alt={alt} {...props} />;
}
