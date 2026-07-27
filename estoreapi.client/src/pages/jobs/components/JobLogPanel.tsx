import { Job, JobLog, statusLabel } from "@/api/jobs";
import { useIsMobile } from "@/hooks/use-mobile";
import { Button } from "@/components/ui/button";
import { ChevronLeft } from "lucide-react";
import { formatDate } from "./JobCard";
import { formatPrice } from "@/lib/formatPrice";

function LogEntry({log}: {log: JobLog}) {
    return (
        <div className="border-b py-2">
            <span className="text-muted-foreground">{formatDate(log.timestamp, { time: true })}</span>
            <ul>
                {log.status && <li><span className="font-semibold">Status change:</span> {statusLabel(log.status)}</li>}
                {log.note && <li><span className="font-semibold">Note change:</span> {log.note}</li>}
                {log.moneyChange && <li><span className="font-semibold">Money change:</span> <span className={log.moneyChange > 0 ? "text-green-600" : "text-destructive"}>{formatPrice(log.moneyChange)}</span></li>}
            </ul>
        </div>
    )
}

interface JobLogPanelProps {
    job: Job;
    onBack: () => void;
}

export default function JobLogPanel({ job, onBack }: JobLogPanelProps) {
    const isMobile = useIsMobile();
    // sort by earliest job first
    // copy a list so it doesn't mutate the original job
    const logs: JobLog[] = [...(job.logs ?? [])].sort(
        (a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime()
    );

    return (
        <div>
            {/** Header */}
            <div className={`flex flex-col ${isMobile ? "p-4" : "pb-4"} border-b`}>
                <div className="flex items-center justify-between">
                    <span className="text-lg text-foreground font-bold">
                        Log of <span className="text-lg text-primary font-mono font-normal">#{job.jobId}</span>
                    </span>
                    <Button variant="outline" size="icon" onClick={onBack}><ChevronLeft /></Button>
                </div>
            </div>
            {/** Log list */}
            <div className={`flex flex-col ${isMobile && "px-4"}`}>
                {logs.map(l => (
                    <LogEntry log={l} />
                ))}
            </div>
        </div>
    )
}