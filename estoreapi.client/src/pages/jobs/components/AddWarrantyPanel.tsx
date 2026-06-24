import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { AlertTriangleIcon, X } from 'lucide-react';
import { useIsMobile } from '@/hooks/use-mobile';
import { Job, addJob } from '@/api/jobs';
import { InfoItem } from './InfoItem';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { formatDate } from './JobCard';
import { getTomorrow } from '@/lib/getTomorrow';

interface AddWarrantyPanelProps {
    original: Job;
    onCancel: () => void;
    onConfirm: () => void;
}

export default function AddWarrantyPanel({ original, onCancel, onConfirm }: AddWarrantyPanelProps) {
    const isMobile = useIsMobile();
    // in months
    const WARRANTY_PERIOD = 3;
    const pickupDate = original.pickupTime ? new Date(original.pickupTime) : null;
    const warrantyExpiry = pickupDate ? new Date(
        new Date(pickupDate).setMonth(pickupDate.getMonth() + WARRANTY_PERIOD)
    ) : null;
    let isExpired = false;
    if (warrantyExpiry && warrantyExpiry.getTime() < Date.now()) {
        isExpired = true;
    }

    async function handleSubmit(e: React.SubmitEvent<HTMLFormElement>) {
        e.preventDefault();
        const form = new FormData(e.currentTarget);
        const pickup = form.get('pickup')?.toString();
        const note = form.get('note')?.toString() ?? '';

        // a warranty job reuses the original's customer and device, but starts with no problems
        await addJob({
            jobId: '',
            customerId: original.customerId,
            deviceId: original.deviceId,
            receiveTime: '',    // default to now
            pickupTime: '',
            estimatedPickupTime: pickup ? new Date(pickup).toISOString() : null,
            note,
            problems: [],
            estimatedPrice: null,
            collectedPrice: null,
            isFinished: false,
            warrantyOfJobId: original.jobId,
        });
        onConfirm();
    }

    return (
        <form onSubmit={handleSubmit} className="w-full h-full overflow-auto">
            {/** header */}
            <div className={`flex items-center justify-between ${isMobile ? "p-4" : "pb-4"} border-b`}>
                <span className="text-lg text-foreground font-bold">
                    Add warranty for <span className="text-lg text-primary font-mono font-normal">#{original.jobId}</span>
                </span>
                <Button type="button" variant="outline" size="icon" onClick={onCancel}><X /></Button>
            </div>
            {/** Alert for expired warranty */}
            { isExpired && warrantyExpiry &&
                <Alert className="w-full mt-4">
                    <AlertTriangleIcon className="!text-destructive" />
                    <AlertTitle className="text-destructive">Warranty expired</AlertTitle>
                    <AlertDescription>
                        Job {original.jobId}'s {WARRANTY_PERIOD} month warranty period expired at {formatDate(warrantyExpiry.toISOString())}.
                    </AlertDescription>
                </Alert>
            }
            {/** editable fields of est.pickup and notes */}
            <div className={`border-b py-4 flex flex-col gap-2 ${isMobile && "px-4"}`}>
                <InfoItem title={"EST. PICKUP TIME"}>
                    <Input type="datetime-local" name="pickup" min={new Date().toISOString().split('T')[0]} defaultValue={getTomorrow()} />
                </InfoItem>
                <InfoItem title={"NOTES"}>
                    <Textarea name="note" />
                </InfoItem>
            </div>
            <div className="flex justify-end gap-2 p-4">
                <Button type="button" variant="outline" onClick={onCancel}>Cancel</Button>
                <Button type="submit">Confirm</Button>
            </div>
        </form>
    );
}