import type { LucideIcon } from "lucide-react";

// for in panels like customer info, device info
export function InfoRow({ icon: Icon, children }: { icon?: LucideIcon, children: React.ReactNode }) {
    return (
        <span className="flex flex-row items-center gap-2">
            {/** w-4 placeholder if no icon for indentation */}
            {Icon ? <Icon className="text-muted-foreground" size={16} /> : <span className="w-4" />}
            {children}
        </span>
    )
}
// for in panels like estimations
export function InfoItem({ title, children }: {title: string, children: React.ReactNode}) {
    return (
        <div className="flex flex-col align-start">
            <span className="text-muted-foreground">{title}</span>
            {children}
        </div>
    )
}