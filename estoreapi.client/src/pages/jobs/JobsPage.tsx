import { useState, useEffect } from 'react';
import { useIsMobile } from '@/hooks/use-mobile';
import { PanelDrawer } from '@/components/PanelDrawer';
import { getJobs, Job } from '@/api/jobs';
import { getCustomers, Customer } from '@/api/customers';
import { getDevices, Device } from '@/api/devices';
import { JobCard } from './components/JobCard';
import { Filter, FilterSearch, FilterSelect } from '@/components/Filter';
import { CircleCheckIcon, Inbox } from 'lucide-react';
import { WorkingPagination } from '@/components/WorkingPagination';

export default function JobsPage({ title }: { title: string }) {
    const isMobile = useIsMobile();
    const [jobs, setJobs] = useState<Job[]>([]);
    const [selectedJob, setSelectedJob] = useState<Job | null>(null);
    const [customers, setCustomers] = useState<Record<string, Customer>>({});
    const [devices, setDevices] = useState<Record<string, Device>>({});
    // filters
    const [selectedFinish, setSelectedFinish] = useState('all');
    const [searchQuery, setSearchQuery] = useState('');
    // pagination
    const [page, setPage] = useState(1);

    // async makes sure all 3 fetches happen together, so no issues like jobs proceeding before customers are fetched
    useEffect(() => {
        async function load() {
            const [jobsList, customersList, devicesList] = await Promise.all([
                getJobs(),
                getCustomers(),
                getDevices(),
            ]);
            setJobs(jobsList);
            setCustomers(Object.fromEntries(customersList.map(c => [c.id, c])));
            setDevices(Object.fromEntries(devicesList.map(d => [d.id, d])));
        }
        load();
    }, []);

    // reset to page 1 whenever the filter or layout changes
    useEffect(() => { setPage(1); }, [isMobile]);
    useEffect(() => { setPage(1); }, [searchQuery]);
    const itemsPerPage = 8;

    // check if the job's customer matches the search query
    function matchesSearch(job: Job) {
        // skip search with no query
        if (!searchQuery) return true;

        const customer = customers[job.customerId];
        const query = searchQuery.toLowerCase();
        // search by customer name or phone
        return customer?.name.toLowerCase().includes(query) || customer?.phone.includes(query);
    }

    // sort the jobs by putting finished jobs after unfinished jobs, 
    // but still retain id asc sort for each section
    const filteredJobs = jobs
        .filter(j => matchesSearch(j))  // match search query (if any) first so search still works
        .sort((a, b) => {   // compare two jobs:
            if (a.isFinished !== b.isFinished) {    // split into in progress & finished sections
                return a.isFinished ? 1 : -1;   // move finished section below in progress section
            } else {
                return parseInt(a.jobId) - parseInt(b.jobId);  // sort by id within each section
            }
        });

    const pagedJobs = filteredJobs.slice((page - 1) * itemsPerPage, page * itemsPerPage);
    
    const toCard = (job: Job) => (
        <JobCard
            key={job.jobId}
            job={job}
            customer={customers[job.customerId] ?? null}
            device={devices[job.deviceId] ?? null}
            isSelected={selectedJob?.jobId === job.jobId}
            onClick={() => setSelectedJob(job)}
        />
    );

    const inProgressCards = pagedJobs.filter(j => !j.isFinished).map(toCard);
    const finishedCards   = pagedJobs.filter(j =>  j.isFinished).map(toCard);

    const pagination = <WorkingPagination className="mt-4" page={page} totalItems={filteredJobs.length} itemsPerPage={itemsPerPage} onPageChange={setPage} />

    const cards = (
        <div className="flex flex-col gap-4 max-w-4xl">
            {(selectedFinish === 'all' || selectedFinish === 'In progress') && inProgressCards.length > 0 && (
                <div className="flex flex-col gap-2">
                    <div className="flex flex-row items-center gap-2">
                        <Inbox className="text-primary" size={16} />
                        <span className="text-primary font-medium">IN PROGRESS</span>
                        <span className="text-muted-foreground">{inProgressCards.length}</span>
                        <hr className="flex-1 border-t border-border" />
                    </div>
                    {inProgressCards}
                </div>
            )}
            {(selectedFinish === 'all' || selectedFinish === 'Finished') && finishedCards.length > 0 && (
                <div className="flex flex-col gap-2">
                    <div className="flex flex-row items-center gap-2">
                        <CircleCheckIcon className="text-foreground" size={16} />
                        <span className="text-foreground font-medium">FINISHED</span>
                        <span className="text-muted-foreground">{finishedCards.length}</span>
                        <hr className="flex-1 border-t border-border" />
                    </div>
                    {finishedCards}
                </div>
            )}
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
                        <FilterSearch placeholder={"Search by customer name or phone..."} onChange={setSearchQuery} />
                        <FilterSelect label="Is finished" options={["In progress", "Finished"]} value={selectedFinish} onChange={setSelectedFinish} />
                    </Filter>
                    {cards}
                </div>
                // desktop
                : <div className="p-8">
                    <h1>{title}</h1>
                    <Filter className="py-4 flex flex-row justify-start gap-2">
                        <FilterSearch placeholder={"Search by customer name or phone..."} onChange={setSearchQuery} />
                        <FilterSelect label="Is finished" options={["In progress", "Finished"]} value={selectedFinish} onChange={setSelectedFinish} />
                    </Filter>
                    {cards}
                </div>
            }
            {pagination}
        </PanelDrawer>
    );
}
