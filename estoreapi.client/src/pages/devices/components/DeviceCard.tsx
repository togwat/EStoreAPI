import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { useIsMobile } from '@/hooks/use-mobile';
import { cn } from '@/lib/utils';
import { Device } from "@/api/devices";

interface DeviceCardProps {
    device: Device;
    isSelected?: boolean;
    onClick: () => void;
}

export function DeviceCard({ device, isSelected, onClick }: DeviceCardProps ) {
    const isMobile = useIsMobile();

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
            onKeyDown={(e) => { if (e.key === "Enter") onClick() }}
        >
            <CardHeader className={cn(isMobile && "flex! flex-row gap-2")}>
                <CardTitle>{device.name}</CardTitle>
                <CardDescription>{device.type}</CardDescription>
            </CardHeader>
        </Card>
    )
}