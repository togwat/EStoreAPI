import MenuHeader from './components/MenuHeader';
import Clock from './components/Clock';
import JobIntake from './components/JobIntake';
import TakingsPerWeek from './components/TakingsPerWeek';
import { useIsMobile } from '@/hooks/use-mobile';

export default function HomePage() {
    const isMobile = useIsMobile();

    return (
        <div className={`flex flex-col gap-8 ${isMobile ? "p-2" : "p-8"}`}>
            <MenuHeader />
            <Clock />
            <TakingsPerWeek />
            <JobIntake />
        </div>
    );
}
