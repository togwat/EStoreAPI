import type { ReactNode } from 'react';
import { useIsMobile } from '@/hooks/use-mobile';
import { cn } from '@/lib/utils';

type PanelDrawerProps = {
    open: boolean;
    drawerContent: ReactNode;
    children: ReactNode;
};

export function PanelDrawer({ open, drawerContent, children }: PanelDrawerProps) {
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
                    {drawerContent}
                </div>
            </div>
        );
    }

    // desktop: flex row push layout
    return (
        <div className="flex flex-1">
            {/** everything else */}
            <div className={cn("flex-1 min-w-0 transition-all")}>
                {children}
            </div>
            {/** drawer - sticky on the outer wrapper so it follows scroll; overflow-hidden clips the width transition */}
            <div className={cn("w-0 overflow-hidden transition-all sticky top-0 h-screen bg-background", open && "w-[min(30rem,55%)] p-8 border-l border-border")}>
                {drawerContent}
            </div>
        </div>
    );
}
