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

export function DeviceCard({ device, onClick }: { device: Device; onClick?: () => void }) {
    const isMobile = useIsMobile();

    return (
        <Card
            size="sm"
            className={cn("w-full border border-border cursor-pointer hover:border-foreground/50 transition-all focus:bg-accent focus:border-ring focus:ring-2 focus:ring-ring/50 focus:text-accent-foreground")}
            onClick={onClick}
            role={"button"}
            tabIndex={0}
            // onKeyDown={(e) => { if (e.key === "Enter") onClick() }}
        >
            <CardHeader className={cn(isMobile && "flex! flex-row gap-2")}>
                <CardTitle>{device.name}</CardTitle>
                <CardDescription>{device.type}</CardDescription>
            </CardHeader>
        </Card>
    )
}