import { Job, JobLog } from "@/api/jobs";
import { useIsMobile } from "@/hooks/use-mobile";
import { Button } from "@/components/ui/button";
import { ChevronLeft } from "lucide-react";

function LogEntry({log}: {log: JobLog}) {
    return (
        <span>{log.timestamp}</span>
    )
}

interface JobLogPanelProps {
    job: Job;
    onBack: () => void;
}

export default function JobLogPanel({ job, onBack }: JobLogPanelProps) {
    const isMobile = useIsMobile();
    const logs: JobLog[] = job.logs ?? [];

    return (
        <div>
            {/** Header */}
            <div className={`flex flex-col gap-2 ${isMobile ? "p-4" : "pb-4"} border-b`}>
                <div className="flex items-center justify-between">
                    <span className="text-lg text-foreground font-bold">
                        Log of <span className="text-lg text-primary font-mono font-normal">#{job.jobId}</span>
                    </span>
                    <Button variant="outline" size="icon" onClick={onBack}><ChevronLeft /></Button>
                </div>
            </div>
            {/** Log list */}
            <div>
                {logs.map(l => (
                    <LogEntry log={l} />
                ))}
            </div>
        </div>
    )
}