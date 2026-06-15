import { ThemeLogo } from '@/components/ThemeIcon';
import { useIsMobile } from '@/hooks/use-mobile';

export default function MenuHeader() {
    const isMobile = useIsMobile();
    return (
        <div className={`flex items-center gap-2 flex-wrap ${isMobile && "mx-auto"}`}>
            <ThemeLogo className={isMobile ? "h-6" : "h-7"} />
            <h1 className={`m-0 ${isMobile ? "text-xl" : "text-2xl"}`}>Management Console</h1>
        </div>
    );
}