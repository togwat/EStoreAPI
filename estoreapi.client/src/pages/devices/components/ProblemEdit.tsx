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
        <Input className="min-w-28 w-full px-2" placeholder={problem.name} value={value}
            onChange={e => { setValue(e.target.value); onEdit(problem.id, e.target.value); }}
        />
    );
});

const EditablePriceCell = memo(function EditablePriceCell({ id, placeholder, onEdit }: {
    id: string;
    placeholder: string;
    onEdit: (id: string, value: string) => void;
}) {
    const [value, setValue] = useState("");
    return (
        <Input className="min-w-18 w-full text-right px-2" placeholder={placeholder} value={value}
            onChange={e => { setValue(e.target.value); onEdit(id, e.target.value); }}
        />
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
    const [editedPartsPrices, setEditedPartsPrices] = useState<Record<string, string>>({});
    const [editedLabourPrices, setEditedLabourPrices] = useState<Record<string, string>>({});
    const [editedRiskCosts, setEditedRiskCosts] = useState<Record<string, string>>({}); 
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
    
    const handleEditPartsPrice = useCallback((id: string, value: string) => {
        setEditedPartsPrices(prev => ({ ...prev, [id]: value }));
    }, []);

    const handleEditLabourPrice = useCallback((id: string, value: string) => {
        setEditedLabourPrices(prev => ({ ...prev, [id]: value}));
    }, []);

    const handleEditRiskCost = useCallback((id: string, value: string) => {
        setEditedRiskCosts(prev => ({ ...prev, [id]: value}));
    }, []);

    const handleDeleteProblem = useCallback((problemId: string) => {
        setProblems(prev => prev.filter(p => p.id !== problemId));
    }, []);

    function handleAddProblem() {
        setProblems(prev => [...prev, { id: `new-${newProblemCounter.current++}`, name: '', price: 0, partsPrice: 0, labourPrice: 0, riskCost: 0 }]);
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
            partsPrice: editedPartsPrices[p.id] ? parseFloat(editedPartsPrices[p.id]) : p.partsPrice,
            labourPrice: editedLabourPrices[p.id] ? parseFloat(editedLabourPrices[p.id]) : p.labourPrice,
            riskCost: editedRiskCosts[p.id] ? parseFloat(editedRiskCosts[p.id]) : p.riskCost
        })),
        cancel: async () => {
            setProblems(await getProblems(deviceId));
            setEditedProblems({});
            setEditedPrices({});
            setEditedPartsPrices({});
            setEditedLabourPrices({});
            setEditedRiskCosts({});
        },
    }), [problems, editedProblems, editedPrices, editedPartsPrices, editedLabourPrices, editedRiskCosts, deviceId]);

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
                ? <EditablePriceCell id={row.original.id} placeholder={formatPrice(row.original.price)} onEdit={handleEditPrice} />
                : <div className="text-right font-medium">{formatPrice(row.getValue("price"))}</div>
        },
        {
            accessorKey: "partsPrice",
            header: () => <div className="text-right">Parts price</div>,
            cell: ({ row }) => isEditing
                ? <EditablePriceCell id={row.original.id} placeholder={formatPrice(row.original.partsPrice)} onEdit={handleEditPartsPrice} />
                : <div className="text-right font-medium">{formatPrice(row.getValue("partsPrice"))}</div>
        },
        {
            accessorKey: "labourPrice",
            header: () => <div className="text-right">{isEditing ? "Lab. price" : "Labour price"}</div>,
            cell: ({ row }) => isEditing
                ? <EditablePriceCell id={row.original.id} placeholder={formatPrice(row.original.labourPrice)} onEdit={handleEditLabourPrice} />
                : <div className="text-right font-medium">{formatPrice(row.getValue("labourPrice"))}</div>
        },
        {
            accessorKey: "riskCost",
            header: () => <div className="text-right">Risk cost</div>,
            cell: ({ row }) => isEditing
                ? <EditablePriceCell id={row.original.id} placeholder={formatPrice(row.original.riskCost)} onEdit={handleEditRiskCost} />
                : <div className="text-right font-medium">{formatPrice(row.getValue("riskCost"))}</div>
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
        <div className={`container mx-auto ${isMobile ? "p-4" : "py-4"} ${isEditing && "[&_td]:px-0.5"}`}>
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
