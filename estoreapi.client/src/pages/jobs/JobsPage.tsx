import { useState, useEffect } from 'react';
import { useIsMobile } from '@/hooks/use-mobile';
import { PanelDrawer } from '@/components/PanelDrawer';
import { getJobs, Job } from '@/api/jobs';
import { getCustomers, Customer } from '@/api/customers';
import { getDevices, Device } from '@/api/devices';
import { JobCard } from './components/JobCard';
import { Filter, FilterSearch, FilterSelect } from '@/components/Filter';
import { CircleCheckIcon, Inbox } from 'lucide-react';

export default function JobsPage({ title }: { title: string }) {
    const isMobile = useIsMobile();
    const [jobs, setJobs] = useState<Job[]>([]);
    const [selectedJob, setSelectedJob] = useState<Job | null>(null);
    const [customers, setCustomers] = useState<Record<string, Customer>>({});
    const [devices, setDevices] = useState<Record<string, Device>>({});
    // filters
    const [selectedFinish, setSelectedFinish] = useState('all');

    useEffect(() => {
        getJobs().then(setJobs);
        getCustomers().then(list => setCustomers(Object.fromEntries(list.map(c => [c.id, c]))));
        getDevices().then(list => setDevices(Object.fromEntries(list.map(d => [d.id, d]))));
    }, []);

    const inProgressJobs = jobs.filter(j => !j.isFinished);
    const finishedJobs = jobs.filter(j => j.isFinished);
    
    const inProgressCards = inProgressJobs.map(job => (
        <JobCard
            key={job.jobId}
            job={job}
            customer={customers[job.customerId] ?? null}
            device={devices[job.deviceId] ?? null}
            isSelected={selectedJob?.jobId === job.jobId}
            onClick={() => setSelectedJob(job)}
        />
    ));

    const finishedCards = finishedJobs.map(job => (
        <JobCard
            key={job.jobId}
            job={job}
            customer={customers[job.customerId] ?? null}
            device={devices[job.deviceId] ?? null}
            isSelected={selectedJob?.jobId === job.jobId}
            onClick={() => setSelectedJob(job)}
        />
    ));

    const cards = (
        <div className="flex flex-col gap-4 max-w-4xl">
            <div className="flex flex-col gap-2">
                <div className="flex flex-row items-center gap-2">
                    <Inbox className="text-primary" size={16} />
                    <span className="text-primary font-medium">IN PROGRESS</span>
                    <span className="text-muted-foreground">{inProgressCards.length}</span>
                    <hr className="flex-1 border-t border-border" />
                </div>
                {inProgressCards}
            </div>
            <div className="flex flex-col gap-2">
                <div className="flex flex-row items-center gap-2">
                    <CircleCheckIcon className="text-foreground" size={16} />
                    <span className="text-foreground font-medium">FINISHED</span>
                    <span className="text-muted-foreground">{finishedCards.length}</span>
                    <hr className="flex-1 border-t border-border" />
                </div>
                {finishedCards}
            </div>
        </div>
    );

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
                    <Filter className="pb-4 flex flex-col gap-2">
                        <FilterSearch placeholder={"Search by customer name or phone..."} />
                        <FilterSelect label="Is finished" options={["In progress", "Finished"]} value={selectedFinish} onChange={setSelectedFinish} />
                    </Filter>
                    {cards}
                </div>
                // desktop
                : <div className="p-8">
                    <h1>{title}</h1>
                    <Filter className="py-4 flex flex-row justify-start gap-2">
                        <FilterSearch placeholder={"Search by customer name or phone..."} />
                        <FilterSelect label="Is finished" options={["In progress", "Finished"]} value={selectedFinish} onChange={setSelectedFinish} />
                    </Filter>
                    {cards}
                </div>
            }
        </PanelDrawer>
    );
}
