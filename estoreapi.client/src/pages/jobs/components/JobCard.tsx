import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { cn } from '@/lib/utils';
import { formatPrice } from '@/lib/formatPrice';
import { Job } from '@/api/jobs';
import { Customer } from '@/api/customers';
import { Device } from '@/api/devices';
import { ClockIcon, CircleCheckIcon, ContactIcon } from "lucide-react";
import { formatPhone } from "@/lib/formatPhone";

export function formatDate(date: string, { year = true, time = false } = {}): string {
    return new Date(date).toLocaleString("en-NZ", {
        day: "numeric",
        month: "short",
        ...(year && { year: "numeric" }),
        ...(time && { hour: "numeric", minute: "2-digit", hour12: true }),
    });
}

interface JobCardProps {
    job: Job;
    customer: Customer;
    device: Device;
    isSelected?: boolean;
    onClick: () => void;
}

export function JobCard({ job, customer, device, isSelected, onClick }: JobCardProps) {
    return (
        <Card
            size="sm"
            className={cn(
                "w-full border border-border cursor-pointer hover:border-foreground/50 transition-all",
                isSelected && "bg-accent border-ring ring-2 ring-ring/50 text-accent-foreground"
            )}
            onClick={onClick}
            role={"button"}
            tabIndex={0}
        >
            <CardHeader>
                <CardTitle className="flex flew-row items-center justify-between">
                    <div className="flex flex-row items-center gap-2">
                        <span className="text-primary text-lg font-mono">#{job.jobId}</span>
                        <span className="font-bold text-lg">{customer.name}</span>
                        <span className="text-muted-foreground flex flex-row items-center gap-1"><ContactIcon size={12}/>{formatPhone(customer.primaryContact)}</span>
                    </div>
                    {job.warrantyOfJobId ?
                        <span>
                            Warranty of <span className="text-primary font-mono font-normal">#{job.warrantyOfJobId}</span>
                        </span>
                    : job.collectedPrice != null ?
                        <span className="text-foreground font-mono">{formatPrice(job.collectedPrice)}</span>
                    : job.estimatedPrice != null ?
                        <span className="text-primary font-mono">{formatPrice(job.estimatedPrice)}</span>
                    : null
                    }
                </CardTitle>
                <CardDescription className="flex flex-row items-center justify-between">
                    <span className="text-foreground font-medium">{device?.name}</span>
                    {job.pickupTime ? 
                        <span className="text-muted-foreground flex flex-row items-center gap-1"><ClockIcon size={12}/>Picked up {formatDate(job.pickupTime)}</span> 
                    : job.estimatedPickupTime ?
                        <span className="text-muted-foreground flex flex-row items-center gap-1"><CircleCheckIcon size={12}/>Due {formatDate(job.estimatedPickupTime)}</span> 
                    : null
                    }
                </CardDescription>
            </CardHeader>
        </Card>
    )
}