import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { useIsMobile } from '@/hooks/use-mobile';
import { cn } from '@/lib/utils';

// follow OutDeviceDTO
export type Device = {
    id: string
    name: string
    type: string
}

export function DeviceCard({ device }: { device: Device }) {
    const isMobile = useIsMobile();

    return (
        <Card size="sm" className="w-full">
            <CardHeader className={cn(isMobile && "flex! flex-row gap-2")}>
                <CardTitle>{device.name}</CardTitle>
                <CardDescription>{device.type}</CardDescription>
            </CardHeader>
        </Card>
    )
}