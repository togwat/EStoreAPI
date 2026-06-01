import { useEffect, useState } from 'react';
import { ColumnDef } from '@tanstack/react-table';
// import { Button } from "@/components/ui/button";
// import {
//   DropdownMenu,
//   DropdownMenuContent,
//   DropdownMenuItem,
//   DropdownMenuLabel,
//   DropdownMenuSeparator,
//   DropdownMenuTrigger,
// } from "@/components/ui/dropdown-menu";
import axios from 'axios';
// import { Trash2Icon } from 'lucide-react';
import { DataTable } from '@/components/ui/data-table';
import { useIsMobile } from '@/hooks/use-mobile';

// follow OutProblemDTO
export type Problem = {
    id: string
    name: string
    price: number
}

const columns: ColumnDef<Problem>[] = [
    {
        accessorKey: "name",
        header: "Problem"
    },
    {
        accessorKey: "price",
        header: () => <div className="text-right">Price</div>,
        cell: ({ row }) => {
            const formattedPrice = new Intl.NumberFormat("en-NZ", {
                style: "currency",
                currency: "NZD"
            }).format(row.getValue("price"))

            return <div className="text-right font-medium">{formattedPrice}</div>
        }
    }
]

async function getProblems(deviceId: number): Promise<Problem[]> {
    const response = await axios.get(`/api/problems/device/${deviceId}`);
    return response.data.map((d: { problemId: string; problemName: string; price: string }) => ({
        id: d.problemId,
        name: d.problemName,
        price: parseFloat(d.price)
    }));
}

export default function DeviceEdit({ deviceId }: { deviceId: number }) {
    const [problems, setProblems] = useState<Problem[]>([]);
    const isMobile = useIsMobile();
    
    useEffect(() => {
        getProblems(deviceId).then(setProblems);
    }, [deviceId]);

    return (
        <div className={`container mx-auto ${isMobile ? "p-4" : "py-4"}`}>
            <DataTable columns={columns} data={problems} />
        </div>
    )
}