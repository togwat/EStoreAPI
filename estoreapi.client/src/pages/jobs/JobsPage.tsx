import { useState, useEffect } from 'react';
import { useIsMobile } from '@/hooks/use-mobile';
import { PanelDrawer } from '@/components/PanelDrawer';
import { Button } from '@/components/ui/button';
import { getJobs, Job } from '@/api/jobs';
import { getCustomers, Customer } from '@/api/customers';
import { getDevices, Device } from '@/api/devices';
import { JobCard, formatPhone } from './components/JobCard';
import { Filter, FilterSearch, FilterSelect } from '@/components/Filter';
import { CircleCheckIcon, Inbox, X, PencilIcon, PhoneIcon, MapPinIcon, MailIcon, type LucideIcon } from 'lucide-react';
import { WorkingPagination } from '@/components/WorkingPagination';
import { formatPrice } from '@/lib/formatPrice';

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

    // reset edit mode when switiching selected job
    useEffect(() => {
        
    });

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

    // changes when selectedJob changes
    const selectedCustomer = selectedJob ? customers[selectedJob.customerId] ?? null : null;
    const selectedDevice = selectedJob ? devices[selectedJob.deviceId] ?? null : null;

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

    // for in panels like customer info, device info
    const InfoRow = ({ icon: Icon, children }: { icon?: LucideIcon, children: React.ReactNode }) => (
        <span className="flex flex-row items-center gap-2">
            {/** w-4 placeholder if no icon for indentation */}
            {Icon ? <Icon className="text-muted-foreground" size={16} /> : <span className="w-4" />}
            {children}
        </span>
    );
    
    return (
        <PanelDrawer
            open={selectedJob !== null}
            drawerContent={selectedJob && (
                <div className="w-full h-full overflow-auto">
                    {/** header */}
                    <div className={`flex items-center justify-between ${isMobile ? "p-4" : "pb-4"} border-b`}>
                        <div className="flex items-center justify-start gap-2">
                            <span className="text-lg text-primary font-mono">#{selectedJob.jobId}</span>
                            <span className="text-lg text-foreground font-bold">{selectedCustomer?.name}</span>
                            <Button variant="ghost" size="icon"><PencilIcon /></Button>
                        </div>
                        <Button variant="outline" size="icon" onClick={() => setSelectedJob(null)}><X /></Button>
                    </div>
                    {/** customer section */}
                    <div className={`border-b py-4 flex flex-col gap-2 ${isMobile && "px-4"}`}>
                        <span className="text-muted-foreground">CUSTOMER</span>
                        <InfoRow icon={PhoneIcon}>{formatPhone(selectedCustomer?.phone)}</InfoRow>
                        {selectedCustomer?.secondPhone && <InfoRow>{formatPhone(selectedCustomer.secondPhone)}</InfoRow>}
                        {selectedCustomer?.email && <InfoRow icon={MailIcon}>{selectedCustomer.email}</InfoRow>}
                        {selectedCustomer?.address && <InfoRow icon={MapPinIcon}>{selectedCustomer.address}</InfoRow>}
                    </div>
                    {/** job's device info */}
                    <div className={`border-b py-4 flex flex-col gap-2 ${isMobile && "px-4"}`}>
                        <span className="text-muted-foreground">DEVICE</span>
                        <div className="flex flex-row items-center gap-2">
                            <span>{selectedDevice?.name}</span>
                            <span className="text-muted-foreground text-sm">{selectedDevice?.type}</span>
                        </div>
                        <span className="text-muted-foreground">PROBLEMS</span>
                        <div className="flex flex-row flex-wrap gap-2">
                            {selectedJob.problems.map(p => (
                                <div className="flex flew-row gap-4 bg-muted text-muted-foreground px-1 rounded-lg">
                                    <span>{p.name}</span>
                                    <span>{formatPrice(p.price)}</span>
                                </div>
                            ))}
                        </div>
                    </div>

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
