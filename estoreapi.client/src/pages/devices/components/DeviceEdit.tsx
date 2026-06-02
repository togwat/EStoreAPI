import { memo, useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { ColumnDef } from '@tanstack/react-table';
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { PlusIcon, Trash2Icon } from 'lucide-react';
import { DataTable } from '@/components/ui/data-table';
import { useIsMobile } from '@/hooks/use-mobile';
import { formatPrice } from '@/lib/formatPrice';
import { Problem, getProblems, updateProblems } from '@/api/problems';


// Own local state so the parent's editedProblems/editedPrices updates don't rebuild columns and remount cells
const EditableNameCell = memo(function EditableNameCell({ problem, onEdit }: {
    problem: Problem;
    onEdit: (id: string, value: string) => void;
}) {
    const [value, setValue] = useState("");
    return (
        <Input className="w-full" placeholder={problem.name} value={value}
            onChange={e => { setValue(e.target.value); onEdit(problem.id, e.target.value); }}
        />
    );
});

const EditablePriceCell = memo(function EditablePriceCell({ problem, onEdit }: {
    problem: Problem;
    onEdit: (id: string, value: string) => void;
}) {
    const [value, setValue] = useState("");
    return (
        <div className="flex justify-end">
            <Input className="w-28 text-right" placeholder={formatPrice(problem.price)} value={value}
                onChange={e => { setValue(e.target.value); onEdit(problem.id, e.target.value); }}
            />
        </div>
    );
});

export default function DeviceEdit({ deviceId, isEditing, onEditingChange }: {
    deviceId: string;
    isEditing: boolean;
    onEditingChange: (v: boolean) => void;
}) {
    const [problems, setProblems] = useState<Problem[]>([]);
    const [editedProblems, setEditedProblems] = useState<Record<string, string>>({});
    const [editedPrices, setEditedPrices] = useState<Record<string, string>>({});
    const isMobile = useIsMobile();
    const newProblemCounter = useRef(0);

    useEffect(() => {
        getProblems(deviceId).then(setProblems);
    }, [deviceId]);

    // useState setters are stable, so these callbacks never change — columns only rebuild when isEditing flips
    const handleEditName = useCallback((id: string, value: string) => {
        setEditedProblems(prev => ({ ...prev, [id]: value }));
    }, []);

    const handleEditPrice = useCallback((id: string, value: string) => {
        setEditedPrices(prev => ({ ...prev, [id]: value }));
    }, []);

    const handleDeleteProblem = useCallback((problemId: string) => {
        setProblems(prev => prev.filter(p => p.id !== problemId));
    }, []);

    function handleAddProblem() {
        setProblems(prev => [...prev, { id: `new-${newProblemCounter.current++}`, name: '', price: 0 }]);
    }

    const columns = useMemo<ColumnDef<Problem>[]>(() => [
        {
            accessorKey: "name",
            header: "Problem",
            cell: ({ row }) => isEditing
                ? <EditableNameCell problem={row.original} onEdit={handleEditName} />
                : <div>{row.original.name}</div>
        },
        {
            accessorKey: "price",
            header: () => <div className="text-right">Price</div>,
            cell: ({ row }) => isEditing
                ? <EditablePriceCell problem={row.original} onEdit={handleEditPrice} />
                : <div className="text-right font-medium">{formatPrice(row.getValue("price"))}</div>
        },
        ...(isEditing ? [{
            id: "actions",
            cell: ({ row }: { row: { original: Problem } }) => (
                <div className="flex justify-end">
                    <Button variant="ghost" size="icon" className="text-destructive p-0" onClick={() => handleDeleteProblem(row.original.id)}>
                        <Trash2Icon />
                    </Button>
                </div>
            )
        }] : [])
    ], [isEditing, handleEditName, handleEditPrice, handleDeleteProblem]);

    async function handleConfirm() {
        const updated = problems.map(p => ({
            ...p,
            id: p.id.startsWith('new-') ? '' : p.id,    // if it is a 'new' id, send it as none
            name: editedProblems[p.id] || p.name,
            price: editedPrices[p.id] ? parseFloat(editedPrices[p.id]) : p.price
        }));

        // call update API
        await updateProblems(deviceId, updated);

        // finally "cancel" to exit edit mode
        await handleCancel();
    }

    async function handleCancel() {
        // set new values from API to get back latest data
        setProblems(await getProblems(deviceId));

        setEditedProblems({});
        setEditedPrices({});
        onEditingChange(false);
    }

    return (
        <div className={`container mx-auto ${isMobile ? "p-4" : "py-4"}`}>
            <DataTable columns={columns} data={problems} />
            {isEditing && (
                <div>
                    <Button variant="outline" className="mt-1 w-full bg-transparent border border-dashed border-foreground/50" onClick={handleAddProblem}><PlusIcon />Add a problem...</Button>
                    <div className="flex justify-end gap-2 pt-4">
                        <Button variant="outline" onClick={handleCancel}>Cancel</Button>
                        <Button onClick={handleConfirm}>Confirm</Button>
                    </div>
                </div>
            )}
        </div>
    );
}
