import JobForm from './components/JobForm';
import { useIsMobile } from '@/hooks/use-mobile';


export default function FormPage({ title }: { title: string }) {
    const isMobile = useIsMobile();

    return (
        <div>
            { !isMobile && <h1 className="text-center mb-4">{title}</h1> }
            <JobForm />
        </div>
    );
}
