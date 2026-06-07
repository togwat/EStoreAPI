import { getTheme } from '../../../lib/theme';
import { themeLogos } from '../../../lib/themeLogos';
import { useIsMobile } from '@/hooks/use-mobile';

export default function MenuHeader() {
    const isMobile = useIsMobile();
    return (
        <div className={`flex items-center gap-2 flex-wrap ${isMobile && "mx-auto"}`}>
            <img src={themeLogos[getTheme()]} className="h-7" />
            <h1 className="m-0 text-2xl">Management Console</h1>
        </div>
    );
}