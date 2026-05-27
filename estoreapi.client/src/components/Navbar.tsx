import { useState } from 'react';
import { Form, ScrollText, TabletSmartphone, Menu, X } from 'lucide-react';
import type { ReactNode } from 'react';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from 'src/components/ui/collapsible';
import { Button } from 'src/components/ui/button';
import { useIsMobile } from '@/hooks/use-mobile';
import { ThemeIcon } from './ThemeIcon';
import NavItem from './NavItem';

interface NavbarProps {
    title: string;
    children?: ReactNode;
}

export function Navbar({ title, children }: NavbarProps) {
    const isMobile = useIsMobile();
    const [open, setOpen] = useState(false);
    const close = () => setOpen(false);

    if (isMobile) {
        return (
            // mobile top navbar with burger
            <div className="flex h-full flex-col">
                <Collapsible open={open} onOpenChange={setOpen} className="sticky top-0 z-5 relative border-b border-border bg-background">
                    <div className="flex items-center justify-between px-2 py-2">
                        <CollapsibleTrigger asChild>
                            <Button variant="outline" size="icon">
                                {open ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
                            </Button>
                        </CollapsibleTrigger>
                        <h1 className="text-xl">{title}</h1>
                        <NavItem to="/" icon={<ThemeIcon className="h-4" />} label="" onClick={close} />
                    </div>
                    <CollapsibleContent className="absolute top-full left-0 right-0 z-40 border-y border-border bg-background">
                        <nav className="flex flex-col gap-1 p-1">
                            <NavItem to="/form" icon={<Form className="h-4 w-4" />} label="Form" horizontal onClick={close} />
                            <NavItem to="/jobs" icon={<ScrollText className="h-4 w-4" />} label="Jobs" horizontal onClick={close} />
                            <NavItem to="/devices" icon={<TabletSmartphone className="h-4 w-4" />} label="Devices" horizontal onClick={close} />
                        </nav>
                    </CollapsibleContent>
                </Collapsible>
                <main className="flex-1 overflow-auto p-4">{children}</main>
            </div>
        );
    }

    // desktop left sidebar
    return (
        <div className="flex h-full">
            <aside className="flex h-full w-20 shrink-0 flex-col border-r border-border bg-background">
                <nav className="flex flex-col gap-1 p-3">
                    <NavItem to="/" icon={<ThemeIcon className="h-8" />} label="" />
                    <NavItem to="/form" icon={<Form className="h-4.5 w-4.5" />} label="Form" />
                    <NavItem to="/jobs" icon={<ScrollText className="h-4.5 w-4.5" />} label="Jobs" />
                    <NavItem to="/devices" icon={<TabletSmartphone className="h-4.5 w-4.5" />} label="Devices" />
                </nav>
            </aside>
            <main className="flex-1 overflow-auto p-8">{children}</main>
        </div>
    );
}
