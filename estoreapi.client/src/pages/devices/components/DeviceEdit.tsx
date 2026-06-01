import { useEffect, useState } from 'react';
import { ColumnDef } from '@tanstack/react-table';
import { Button } from "@/components/ui/button";
import axios from 'axios';
import { Trash2Icon } from 'lucide-react';
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
    },
    {
        id: "actions",
        cell: ({ row }) => {
            const problem = row.original;   // get data of selected row

            return (
                <div className="flex justify-end">
                    <Button variant="ghost" size="icon" className="text-destructive text-right p-0" onClick={() => deleteProblem(problem.id)}>
                        <Trash2Icon />
                    </Button>
                </div>
            )
        }
    }
]

async function getProblems(deviceId: string): Promise<Problem[]> {
    const response = await axios.get(`/api/problems/device/${deviceId}`);
    return response.data.map((d: { problemId: string; problemName: string; price: string }) => ({
        id: d.problemId,
        name: d.problemName,
        price: parseFloat(d.price)
    }));
}

async function deleteProblem(problemId: string) {
    // TODO
    console.log(problemId);
}

export default function DeviceEdit({ deviceId }: { deviceId: string }) {
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