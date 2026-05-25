import { useIsMobile } from '@/hooks/use-mobile';

export default function DevicesPage({ title }: { title: string }) {
    const isMobile = useIsMobile();

    return (
        <div>
            { !isMobile && <h1>{title}</h1> }
        </div>
    );
}