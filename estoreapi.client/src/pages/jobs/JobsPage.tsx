import { useState, useEffect } from 'react';
import { useIsMobile } from '@/hooks/use-mobile';
import { PanelDrawer } from '@/components/PanelDrawer';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { getJobs, updateJob, Job, JobStatus } from '@/api/jobs';
import { getCustomers, Customer } from '@/api/customers';
import { getDevices, Device } from '@/api/devices';
import { JobCard, formatDate } from './components/JobCard';
import { Filter, FilterSearch, FilterSelect } from '@/components/Filter';
import { CircleCheckIcon, Inbox, X, PencilIcon, PhoneIcon, MapPinIcon, MailIcon, ContactIcon } from 'lucide-react';
import { WorkingPagination } from '@/components/WorkingPagination';
import { formatPrice } from '@/lib/formatPrice';
import { formatPhone } from '@/lib/formatPhone';
import { toLocalDatetimeInputValue } from '@/lib/toLocalDatetime';
import { toLocalDateKey } from '@/lib/localDateKey';
import { Textarea } from '@/components/ui/textarea';
import { InfoItem, InfoRow } from './components/InfoItem';
import AddWarrantyPanel from './components/AddWarrantyPanel';

export default function JobsPage({ title }: { title: string }) {
    const isMobile = useIsMobile();
    const [jobs, setJobs] = useState<Job[]>([]);
    const [selectedJob, setSelectedJob] = useState<Job | null>(null);
    const [customers, setCustomers] = useState<Record<string, Customer>>({});
    const [devices, setDevices] = useState<Record<string, Device>>({});
    // filters
    const [selectedStatus, setSelectedStatus] = useState('all');
    const [searchQuery, setSearchQuery] = useState('');
    // pagination
    const [page, setPage] = useState(1);
    // editing/updates
    const [isEditing, setIsEditing] = useState(false);
    const [editedStatus, setEditedStatus] = useState<JobStatus | null>(null);
    const [editedPickupTime, setEditedPickupTime] = useState('');
    const [editedCollectedPrice, setEditedCollectedPrice] = useState('');
    const [editedNote, setEditedNote] = useState('');
    // warranty
    const [isAddingWarranty, setIsAddingWarranty] = useState(false);

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

    // reset edit mode when switching selected jobs
    useEffect(() => {
        handleCancel();
    }, [selectedJob?.jobId]);

    async function handleConfirm() {
        // process input
        const newPickupTime = editedPickupTime ? new Date(editedPickupTime).toISOString() : null;

        // only 4 fields should be updated
        const updatedJob: Job = {...selectedJob!,
            status: editedStatus ?? selectedJob!.status,
            pickupTime: newPickupTime || selectedJob!.pickupTime,
            collectedPrice: editedCollectedPrice ? parseFloat(editedCollectedPrice) : selectedJob!.collectedPrice,
            note: editedNote || selectedJob!.note
        }

        await updateJob(selectedJob!.jobId, updatedJob);
        await refreshJobs();
        handleCancel();
    }

    function handleCancel() {
        setIsEditing(false);
        setEditedStatus(null);
        setEditedPickupTime('');
        setEditedCollectedPrice('');
        setEditedNote('');
        setIsAddingWarranty(false);
    }

    // get latest data to refresh
    async function refreshJobs() {
        const refreshed = await getJobs();
        setJobs(refreshed);
        // keep open the original job's panel
        setSelectedJob(prev => prev ? refreshed.find(j => j.jobId === prev.jobId) ?? prev : null);
    }

    // reset to page 1 whenever the filter or layout changes
    useEffect(() => { setPage(1); }, [isMobile]);
    useEffect(() => { setPage(1); }, [searchQuery]);
    useEffect(() => { setPage(1); }, [selectedStatus])
    const itemsPerPage = 8;

    // check if the job's customer or device matches the search query
    function matchesSearch(job: Job) {
        // skip search with no query
        if (!searchQuery) return true;

        const customer = customers[job.customerId];
        const device = devices[job.deviceId];
        const query = searchQuery.toLowerCase();
        // search by customer name or primary contact, or by device name
        return customer?.name.toLowerCase().includes(query) 
            || customer?.primaryContact.includes(query)
            || device?.name.toLowerCase().includes(query);
    }

    // map enum key to string if there is a different string representation
    const statusLabels: Partial<Record<JobStatus, string>> = {
        [JobStatus.InProgress]: 'In progress',
    }
    const statusLabel = (status: JobStatus) => statusLabels[status] ?? status;

    // check if the job's status matches the status dropdown filter
    function matchesStatus(job: Job) {
        // skip filter with all statuses
        if (selectedStatus === 'all') return true;
        // return true if job's status matches selected filter status
        return statusLabel(job.status) === selectedStatus;
    }

    // sort the jobs by putting finished jobs after unfinished jobs, 
    // but still retain id asc sort for each section
    const filteredJobs = jobs
        .filter(j => matchesSearch(j))  // match search query (if any) first so search still works
        .filter(j => matchesStatus(j))  // match status filter
        .sort((a, b) => {   // compare two jobs:
            if (a.status !== b.status) {    // split into in progress & finished sections
                return a.status === JobStatus.Finished ? 1 : -1;   // move finished section below in progress section
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

    const inProgressCards = pagedJobs.filter(j => j.status !== JobStatus.Finished).map(toCard);
    const finishedCards = pagedJobs.filter(j => j.status === JobStatus.Finished).map(toCard);

    const pagination = <WorkingPagination className="mb-4" page={page} totalItems={filteredJobs.length} itemsPerPage={itemsPerPage} onPageChange={setPage} />

    const cards = (
        <div className="flex flex-col gap-4 max-w-4xl">
            {inProgressCards.length > 0 && (
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
            {finishedCards.length > 0 && (
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

    const editJobPanel = (selectedJob: Job) => (
        <div className="w-full h-full overflow-auto">
            {/** header */}
            <div className={`flex flex-col gap-2 ${isMobile ? "p-4" : "pb-4"} border-b`}>
                <div className="flex items-center justify-between">
                    <div className="flex items-center justify-start gap-2">
                        <span className="text-lg text-primary font-mono">#{selectedJob.jobId}</span>
                        <span className="text-lg text-foreground font-bold">{selectedCustomer?.name}</span>
                    </div>
                    <Button variant="outline" size="icon" onClick={() => setSelectedJob(null)}><X /></Button>
                </div>
                {selectedJob.warrantyOfJobId &&
                    <span>
                        Warranty of <span className="text-primary font-mono font-normal">#{selectedJob.warrantyOfJobId}</span>
                    </span>
                }
            </div>
            {/** customer section */}
            <div className={`border-b py-4 flex flex-col gap-2 ${isMobile && "px-4"}`}>
                <span className="text-muted-foreground">CUSTOMER</span>
                <InfoRow icon={ContactIcon}>
                    <span>{formatPhone(selectedCustomer?.primaryContact)}</span>
                </InfoRow>
                {selectedCustomer?.phone && <InfoRow icon={PhoneIcon}>
                    <span>{formatPhone(selectedCustomer.phone)}</span>
                </InfoRow>}
                {selectedCustomer?.email && <InfoRow icon={MailIcon}>
                    <span>{selectedCustomer.email}</span>
                </InfoRow>}
                {selectedCustomer?.address && <InfoRow icon={MapPinIcon}>
                    <span>{selectedCustomer.address}</span>
                </InfoRow>}
            </div>
            {/** job's device info */}
            <div className={`border-b py-4 flex flex-col gap-2 ${isMobile && "px-4"}`}>
                <span className="text-muted-foreground">DEVICE</span>
                <div className="flex flex-row items-center gap-2">
                    <span>{selectedDevice?.name}</span>
                    <span className="text-muted-foreground text-sm">{selectedDevice?.type}</span>
                </div>
                {selectedJob.problems.length >= 1 &&
                <div>
                    <span className="text-muted-foreground">PROBLEMS</span>
                    <div className="flex flex-row flex-wrap gap-2">
                        {selectedJob.problems.map(p => (
                            <div key={p.id} className="flex flew-row gap-4 bg-muted text-muted-foreground px-1 rounded-lg">
                                <span>{p.name}</span>
                                <span>{formatPrice(p.price)}</span>
                            </div>
                        ))}
                    </div>
                </div>
                }
            </div>
            {/** Estimation info */}
            <div className={`border-b py-4 flex flex-col gap-2 ${isMobile && "px-4"}`}>
                <div className="grid grid-cols-2 gap-2">
                    <InfoItem title={"RECEIVED"}>
                        <span>{formatDate(selectedJob.receiveTime, { year: false, time: true })}</span>
                    </InfoItem>
                    {selectedJob.estimatedPickupTime && <InfoItem title="EST. PICKUP">
                        <span>{formatDate(selectedJob.estimatedPickupTime, { year: false, time: true })}</span>
                    </InfoItem>}
                    {/** Warranty jobs have no price */}
                    {selectedJob.estimatedPrice != null && selectedJob.warrantyOfJobId == null &&
                     <InfoItem title="EST. PRICE">
                        <span>{formatPrice(selectedJob.estimatedPrice)}</span>
                    </InfoItem>}
                </div>
            </div>
            {/** editable info */}
            <div className={`py-4 flex flex-col gap-2 ${isMobile && "px-4"}`}>
                <div className="flex items-center justify-start gap-2">
                    <span className={isEditing ? "text-primary" : "text-foreground"}>UPDATE</span>
                    {!isEditing && <Button variant="ghost" size="icon" onClick={() => {
                        setIsEditing(true);
                        setEditedStatus(selectedJob.status);
                        setEditedPickupTime(selectedJob.pickupTime ? toLocalDatetimeInputValue(selectedJob.pickupTime) : '');
                        setEditedCollectedPrice(selectedJob.collectedPrice != null ? String(selectedJob.collectedPrice) : '');
                        setEditedNote(selectedJob.note ?? '');
                    }}><PencilIcon /></Button>}
                </div>
                <InfoItem title={"STATUS"}>
                    {isEditing ? 
                    <div className="w-fit rounded-full border border-border bg-input">
                        <Button className="rounded-full" variant={editedStatus === JobStatus.InProgress ? "default" : "ghost"} onClick={() => setEditedStatus(JobStatus.InProgress)}>In progress</Button>
                        <Button className="rounded-full" variant={editedStatus === JobStatus.Finished ? "default" : "ghost"} onClick={() => setEditedStatus(JobStatus.Finished)}>Finished</Button>
                    </div>
                    : <span>{selectedJob.status === JobStatus.Finished ? "Finished" : "In progress"}</span>
                    }
                </InfoItem>
                <InfoItem title={"PICKUP TIME"}>
                    {isEditing ? <Input type="datetime-local" value={editedPickupTime} onChange={e => setEditedPickupTime(e.target.value)} max={toLocalDateKey(new Date())}/>
                    : selectedJob.pickupTime ? <span>{formatDate(selectedJob.pickupTime, { time: true })}</span> : <span className="text-muted-foreground">Not picked up yet</span>
                    }
                    
                </InfoItem>
                {/** Warranty jobs have no price */}
                {selectedJob.warrantyOfJobId == null &&
                    <InfoItem title={"COLLECTED PRICE"}>
                        {isEditing ? <Input type="number" value={editedCollectedPrice} onChange={e => setEditedCollectedPrice(e.target.value)}/>
                        : selectedJob.collectedPrice != null ? <span>{formatPrice(selectedJob.collectedPrice)}</span> : <span className="text-muted-foreground">---</span>
                        }
                    </InfoItem>
                }
                <InfoItem title={"NOTES"}>
                    {isEditing ? <Textarea value={editedNote} onChange={e => setEditedNote(e.target.value)} />
                    : selectedJob.note ? <p>{selectedJob.note}</p> : <span className="text-muted-foreground">---</span>
                    }
                </InfoItem>
                {isEditing && 
                    <div className="flex justify-end gap-2 p-4">
                        <Button variant="outline" onClick={handleCancel}>Cancel</Button>
                        <Button onClick={handleConfirm}>Confirm</Button>
                    </div>
                }
            </div>
            {/** 
             * Create warranty job 
             * Shows up on finished jobs only
             * Hidden when in edit mode to not overcrowd
             * */}
            {selectedJob.status === JobStatus.Finished && !isEditing &&
                <div className="flex justify-end p-4">
                    <Button onClick={() => {setIsAddingWarranty(true)}}>Create warranty job</Button>
                </div>
            }
        </div>
    )

    return (
        <PanelDrawer
            open={selectedJob !== null}
            drawerContent={selectedJob && (isAddingWarranty
                ? <AddWarrantyPanel
                    original={selectedJob}
                    onClose={() => setSelectedJob(null)}
                    onCancel={handleCancel}
                    onConfirm={async () => { await refreshJobs(); handleCancel(); }}
                  />
                : editJobPanel(selectedJob))}
        >
            {/** main body */}
            {isMobile
                // mobile
                ? <div className="flex flex-col gap-2">
                    <Filter className="pb-4 flex flex-col gap-2">
                        <FilterSearch placeholder={"Search jobs..."} onChange={setSearchQuery} />
                        <FilterSelect label="Status" options={Object.values(JobStatus).map(statusLabel)} value={selectedStatus} onChange={setSelectedStatus} />
                    </Filter>
                    {cards}
                    {pagination}
                </div>
                // desktop
                : <div className="p-8">
                    <h1>{title}</h1>
                    <Filter className="py-4 flex flex-row justify-start gap-2">
                        <FilterSearch placeholder={"Search jobs..."} onChange={setSearchQuery} />
                        <FilterSelect label="Status" options={Object.values(JobStatus).map(statusLabel)} value={selectedStatus} onChange={setSelectedStatus} />
                    </Filter>
                    <div className="mb-4">{cards}</div>
                    {pagination}
                </div>
            }
        </PanelDrawer>
    );
}
