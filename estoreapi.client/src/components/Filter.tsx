import { InputGroup, InputGroupAddon, InputGroupInput } from '@/components/ui/input-group';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Search, ArrowUp, ArrowDown } from 'lucide-react';
import { ReactElement } from 'react'
import { Button } from '@/components/ui/button';
import { ButtonGroup } from '@/components/ui/button-group';

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

interface FilterSortProps {
    label: string
    options: string[]
    value: string
    onChange: (value: string) => void
    direction: "asc" | "desc"
    onDirectionChange: (direction: "asc" | "desc") => void
}

// assuming asc/desc
export function FilterSort({ label, options, value, onChange, direction, onDirectionChange }: FilterSortProps) {
    return (
        <ButtonGroup>
            <Select value={value} onValueChange={onChange}>
                <SelectTrigger>
                    <SelectValue placeholder={label} />
                </SelectTrigger>
                <SelectContent position="popper">
                    {options.map(type => (
                        <SelectItem key={type} value={type}>{type}</SelectItem>
                    ))}
                </SelectContent>
            </Select>
            <Button variant="outline" size="icon" onClick={
                () => onDirectionChange(direction === "asc" ? "desc" : "asc")
            }>{direction === "asc" ? <ArrowUp /> : <ArrowDown />}</Button>
        </ButtonGroup>
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
        <div>
            {children}
        </div>
    )
}