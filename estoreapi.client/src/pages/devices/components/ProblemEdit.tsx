import { forwardRef, memo, useCallback, useEffect, useImperativeHandle, useMemo, useRef, useState } from 'react';
import { ColumnDef } from '@tanstack/react-table';
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { PlusIcon, Trash2Icon } from 'lucide-react';
import { DataTable } from '@/components/ui/data-table';
import { useIsMobile } from '@/hooks/use-mobile';
import { formatPrice } from '@/lib/formatPrice';
import { Problem, getProblems } from '@/api/problems';

// The Confirm/Cancel buttons live in DevicesPage (the parent), but they need to trigger
// actions that depend on state inside this component (problems, edits).
//
// The React way to let a parent call a child's functions is:
//   1. forwardRef  — allows the parent to pass a `ref` prop to this component
//   2. useImperativeHandle — lets us choose exactly which functions that ref exposes
//
// The parent creates a ref with useRef<ProblemEditHandle>(), passes it as ref={...},
// then calls ref.current.getUpdatedProblems() / ref.current.cancel() from its buttons.
export interface ProblemEditHandle {
    getUpdatedProblems: () => Problem[];
    cancel: () => Promise<void>;
}

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

// forwardRef wraps the component so it can accept a ref prop from the parent
const ProblemEdit = forwardRef<ProblemEditHandle, {
    deviceId: string;
    isEditing: boolean;
}>(function ProblemEdit({ deviceId, isEditing }, ref) {
    const [problems, setProblems] = useState<Problem[]>([]);
    const [editedProblems, setEditedProblems] = useState<Record<string, string>>({});
    const [editedPrices, setEditedPrices] = useState<Record<string, string>>({});
    const isMobile = useIsMobile();
    const newProblemCounter = useRef(0);

    // only retrieve problems if not in add mode (there is device id)
    useEffect(() => {
        if (deviceId) getProblems(deviceId).then(setProblems);
        else setProblems([]);
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

    // Expose these two functions to the parent via the forwarded ref.
    // The dependency array works like useMemo — re-creates the handle whenever the
    // closed-over state changes so the parent always gets fresh data.
    useImperativeHandle(ref, () => ({
        getUpdatedProblems: () => problems.map(p => ({
            ...p,
            id: p.id.startsWith('new-') ? '' : p.id,    // if it is a 'new' id, send it as none
            name: editedProblems[p.id] || p.name,
            price: editedPrices[p.id] ? parseFloat(editedPrices[p.id]) : p.price,
        })),
        cancel: async () => {
            setProblems(await getProblems(deviceId));
            setEditedProblems({});
            setEditedPrices({});
        },
    }), [problems, editedProblems, editedPrices, deviceId]);

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

    return (
        <div className={`container mx-auto ${isMobile ? "p-4" : "py-4"}`}>
            <DataTable columns={columns} data={problems} />
            {isEditing && (
                <Button variant="outline" className="mt-1 w-full bg-transparent border border-dashed border-foreground/50" onClick={handleAddProblem}>
                    <PlusIcon />Add a problem...
                </Button>
            )}
        </div>
    );
});

export default ProblemEdit;
