import { useIsMobile } from '@/hooks/use-mobile';

export default function SettingsPage({ title }: { title: string }) {
    const isMobile = useIsMobile();

    return (
        <div className={`flex flex-col ${isMobile ? "p-2" : "p-8"}`}>
             { !isMobile && <h1>{title}</h1> }
        </div>
    );
}
