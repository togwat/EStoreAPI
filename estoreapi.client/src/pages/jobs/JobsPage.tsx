import { useState, useEffect } from 'react';
import { useIsMobile } from '@/hooks/use-mobile';
import { PanelDrawer } from '@/components/PanelDrawer';
import { getJobs, Job } from '@/api/jobs';

export default function JobsPage({ title }: { title: string }) {
    const isMobile = useIsMobile();
    const [jobs, setJobs] = useState<Job[]>([]);                        // @ts-ignore
    const [selectedJob, setSelectedJob] = useState<Job | null>(null);   // @ts-ignore

    useEffect(() => {
        getJobs().then(setJobs);
    }, []);
    
    console.log(jobs);

    return (
        <PanelDrawer
            open={selectedJob !== null}
            drawerContent={selectedJob && (
                <div className="w-full h-full overflow-auto">
                </div>
            )}
        >
            {/** main body */}
            {isMobile
                // mobile
                ? <div className="flex flex-col gap-2">
                </div>
                // desktop
                : <div className="p-8">
                    <h1>{title}</h1>
                </div>
            }
        </PanelDrawer>
    );
}
