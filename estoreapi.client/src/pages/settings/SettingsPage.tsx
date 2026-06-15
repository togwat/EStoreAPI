import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Field, FieldLabel } from '@/components/ui/field';
import { useIsMobile } from '@/hooks/use-mobile';
import { getTheme, setTheme, ThemeName } from '@/lib/theme';
import { useState } from 'react';

// follow themes in @/lib/theme
const themeLabels: Record<ThemeName, string> = {
    "rustic-leather": "Rustic Leather",
    "generic-dark": "Generic Dark",
    "generic-light": "Generic Light"
}

export default function SettingsPage({ title }: { title: string }) {
    const isMobile = useIsMobile();
    const [selectedTheme, setSelectedTheme] = useState(getTheme);
    
    function handleSelectTheme(value: ThemeName) {
        setSelectedTheme(value);
        setTheme(value);
    }

    return (
        <div className={`flex flex-col ${isMobile ? "p-2" : "p-8"}`}>
             { !isMobile && <h1 className="pb-4">{title}</h1> }
             {/** Theme selector setting */}
             <div className="flex flex-col gap-2 max-w-sm">
                <Field>
                    <FieldLabel>Change theme</FieldLabel>
                    <Select value={selectedTheme} onValueChange={handleSelectTheme}>
                        <SelectTrigger>
                            <SelectValue />
                        </SelectTrigger>
                        <SelectContent position="popper">
                            {Object.entries(themeLabels).map(([value, label]) => (
                                <SelectItem key={value} value={value}>{label}</SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                </Field>
             </div>
        </div>
    );
}
