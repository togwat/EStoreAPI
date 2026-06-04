import { useState, useEffect } from 'react';
import { useIsMobile } from '@/hooks/use-mobile';
import { PanelDrawer } from '@/components/PanelDrawer';
import { getJobs, Job } from '@/api/jobs';
import { getCustomers, Customer } from '@/api/customers';
import { getDevices, Device } from '@/api/devices';
import { JobCard } from './components/JobCard';
import { Filter, FilterSearch, FilterSelect } from '@/components/Filter';

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
    
    const filteredJobs = selectedFinish === 'Finished' ? jobs.filter(j => j.isFinished)
        : selectedFinish === 'In progress' ? jobs.filter(j => !j.isFinished)
        : jobs;

    const cards = filteredJobs.map(job => (
        <JobCard
            key={job.jobId}
            job={job}
            customer={customers[job.customerId] ?? null}
            device={devices[job.deviceId] ?? null}
            isSelected={selectedJob?.jobId === job.jobId}
            onClick={() => setSelectedJob(job)}
        />
    ));

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
                    <div className="flex flex-col gap-2">
                       {cards}
                    </div>
                </div>
            }
        </PanelDrawer>
    );
}
