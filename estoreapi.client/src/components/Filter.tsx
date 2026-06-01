import { InputGroup, InputGroupAddon, InputGroupInput } from '@/components/ui/input-group';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Search } from 'lucide-react';
import { ReactElement } from 'react'

interface FilterSearchProps {
    placeholder: string
}

export function FilterSearch({ placeholder }: FilterSearchProps) {
    return (
        <InputGroup className="max-w-xs">
            <InputGroupInput placeholder={placeholder} />
            <InputGroupAddon>
                <Search />
            </InputGroupAddon>
        </InputGroup>
    )
}

interface FilterSelectProps {
    label: string
    options: string[]
    value: string
    onChange: (value: string) => void
}

export function FilterSelect({ label, options, value, onChange }: FilterSelectProps) {
    return (
        <Select value={value} onValueChange={onChange}>
            <SelectTrigger>
                <SelectValue placeholder={label} />
            </SelectTrigger>
            <SelectContent position="popper">
                <SelectItem value="all">All</SelectItem>
                {options.map(type => (
                    <SelectItem key={type} value={type}>{type}</SelectItem>
                ))}
            </SelectContent>
        </Select>
    )
}

// only allow Filter to have filter variants as children
type FilterChild = 
    | ReactElement<FilterSearchProps, typeof FilterSearch>
    | ReactElement<FilterSelectProps, typeof FilterSelect>

interface FilterProps {
    children: FilterChild | FilterChild[]
}

export function Filter({ children }: FilterProps) {
    return (
        <div className="py-4 flex flex-row justify-start gap-2">
            {children}
        </div>
    )
}