// import { Button } from '@/components/ui/button';
import { InputGroup, InputGroupAddon, InputGroupInput } from '@/components/ui/input-group';
import { Search } from 'lucide-react';

interface FilterProps {
    inputPlaceholder: string;
    // columnSelect: string;
}


export default function Filter({ inputPlaceholder }: FilterProps) {
    return (
        <div className="py-4">
            <InputGroup className="max-w-xs">
                <InputGroupInput placeholder={inputPlaceholder} />
                <InputGroupAddon>
                    <Search />
                </InputGroupAddon>
            </InputGroup>
        </div>
    )
}