import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"

// follow OutDeviceDTO
export type Device = {
    id: string
    name: string
    type: string
}

export function DeviceCard({ device }: { device: Device }) {
    return (
        <Card size="sm" className="w-full">
            <CardHeader>
                <CardTitle>{device.name}</CardTitle>
                <CardDescription>{device.type}</CardDescription>
            </CardHeader>
        </Card>
    )
}