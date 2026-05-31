import { ColumnDef } from "@tanstack/react-table";

// follow OutDeviceDTO
export type Device = {
    id: string
    name: string
    type: string
}

export const columns: ColumnDef<Device>[] = [
    {
        accessorKey: "name",
        header: "Device model"
    },
    {
        accessorKey: "type",
        header: "Device Type"
    }
]