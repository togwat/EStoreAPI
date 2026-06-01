import type { ReactNode } from 'react';
import { useIsMobile } from '@/hooks/use-mobile';
import { cn } from '@/lib/utils';
import { Button } from './ui/button';
import { X } from 'lucide-react';

type PanelDrawerProps = {
    open: boolean;
    onClose: () => void;
    title?: string;
    drawerContent: ReactNode;
    children: ReactNode;
};

export function PanelDrawer({ open, onClose, title, drawerContent, children }: PanelDrawerProps) {
    const isMobile = useIsMobile();

    if (isMobile) {
        return (
            <div>
                {children}
                {/** drawer anchors to the nearest positioned ancestor (left panel in AssistantSidebar) */}
                <div
                    className={cn(
                        "absolute inset-0 z-4 bg-background overflow-auto transition-transform top-12",
                        open ? "translate-y-0" : "translate-y-full"
                    )}
                >
                    {/** header */}
                    <div className="flex items-center justify-between p-4 border-b">
                        <span className="text-base font-medium">{title}</span>
                        <Button variant="outline" size="icon" onClick={onClose}><X /></Button>
                    </div>
                    {drawerContent}
                </div>
            </div>
        );
    }

    // desktop: flex row push layout
    return (
        <div className="flex flex-1">
            {/** everything else */}
            <div className={cn("flex-1 min-w-0 transition-all", open && "mr-8")}>
                {children}
            </div>
            {/** drawer */}
            <div className={cn("w-0 h-full overflow-hidden transition-all", open && "w-120")}>
                <div className="w-120 h-full overflow-auto sticky">
                    {/** header */}
                    <div className="flex items-center justify-between py-4 border-b">
                        <span className="text-base font-medium">{title}</span>
                        <Button variant="outline" size="icon" onClick={onClose}><X /></Button>
                    </div>
                    {drawerContent}
                </div>
            </div>
        </div>
    );
}
