import { useIsMobile } from '@/hooks/use-mobile';

function DevicesPage({ title }: { title: string }) {
    const isMobile = useIsMobile();

    return (
        <div>
            { !isMobile && <h1>{title}</h1> }
        </div>
    );
}

export default DevicesPage;