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
import { PhoneIcon, ClockIcon, CircleCheckIcon } from "lucide-react";

// takes continuous string of digits and formats it into phone no. with spaces
// e.g. 0221234567 -> 022 123 4567
function formatPhone(phone: string): string {
  const digits = phone.replace(/\D/g, "")

    // +64 international prefix -> replace with leading 0
    const normalized = digits.startsWith("64") && digits.length > 9
        ? "0" + digits.slice(2)
        : digits;

    // Mobile: 021, 022, 027, 028, 029 -> 3 + 3 + 4
    if (/^02\d/.test(normalized)) {
        return normalized.replace(/^(0\d{2})(\d{3})(\d{1,4})$/, "$1 $2 $3");
    }

    // Landline: 09, 04, 03, 07, 06, etc. -> 2 + 3 + 4
    if (/^0[3-9]/.test(normalized)) {
        return normalized.replace(/^(0\d)(\d{3})(\d{1,4})$/, "$1 $2 $3");
    }

    // 0800 / 0900 freephone -> 4 + 3 + 3
    if (/^0[89]00/.test(normalized)) {
        return normalized.replace(/^(0[89]00)(\d{3})(\d{1,3})$/, "$1 $2 $3");
    }

    return phone;   // unrecognised, return as-is
}

function formatDate(date: string): string {
    return new Date(date).toLocaleDateString("en-NZ", {
        day: "numeric",
        month: "short",
        year: "numeric"
    })
}

interface JobCardProps {
    job: Job;
    customer: Customer;
    device: Device;
    isSelected?: boolean;
    onClick: () => void;
}

export function JobCard({ job, customer, device, isSelected, onClick }: JobCardProps) {
    console.log(job.collectedPrice);

    return (
        <Card
            size="sm"
            className={cn(
                "max-w-4xl border border-border cursor-pointer hover:border-foreground/50 transition-all",
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
                        <span className="font-bold text-lg">{customer?.name}</span>
                        <span className="text-muted-foreground flex flex-row items-center gap-1"><PhoneIcon size={12}/>{formatPhone(customer.phone)}</span>
                    </div>
                    {job.collectedPrice && !isNaN(parseFloat(job.collectedPrice)) ?
                        <span className="text-foreground font-mono">{formatPrice(parseFloat(job.collectedPrice))}</span>
                    : job.estimatedPrice && !isNaN(parseFloat(job.estimatedPrice)) ?
                        <span className="text-primary font-mono">{formatPrice(parseFloat(job.estimatedPrice))}</span>
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