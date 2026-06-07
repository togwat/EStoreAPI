import MenuHeader from './components/MenuHeader';
import { JobIntakeChart } from './components/JobIntakeChart';
import { useIsMobile } from '@/hooks/use-mobile';
import { TakingsPerWeek } from './components/TakingsPerWeek';

export default function HomePage() {
    const isMobile = useIsMobile();

    return (
        <div className={`flex flex-col gap-8 ${isMobile ? "p-2" : "p-8"}`}>
            <MenuHeader />
            <TakingsPerWeek />
            <JobIntakeChart />
        </div>
    );
}
