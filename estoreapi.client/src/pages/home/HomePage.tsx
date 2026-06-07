import MenuHeader from './components/MenuHeader';
import { JobIntakeChart } from './components/JobIntakeChart';
import { useIsMobile } from '@/hooks/use-mobile';

export default function HomePage() {
    const isMobile = useIsMobile();

    return (
        <div className={`flex flex-col gap-8 ${isMobile ? "p-2" : "p-8"}`}>
            <MenuHeader />
            <JobIntakeChart />
        </div>
    );
}
