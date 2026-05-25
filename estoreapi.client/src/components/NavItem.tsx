import { NavLink } from 'react-router-dom';
import type { ReactNode } from 'react';
import { cn } from 'src/lib/utils';

interface NavItemProps {
    to: string;
    icon: ReactNode;
    label: string;
    horizontal?: boolean;
    onClick?: () => void;
}

// horizontal for mobile, vertical for desktop

export default function NavItem({ to, icon, label, horizontal, onClick }: NavItemProps) {
    return (
        <NavLink
            to={to}
            end={to === '/'}
            onClick={onClick}
            className={({ isActive }) =>
                cn(
                    'flex rounded-md px-3 py-2 transition-colors',
                    horizontal ? 'flex-row items-center gap-3' : 'flex-col items-center gap-1',
                    isActive
                        ? 'bg-accent text-accent-foreground'
                        : 'text-foreground hover:bg-accent hover:text-accent-foreground'
                )
            }
        >
            {icon}
            {label && <span className="text-xs font-medium">{label}</span>}
        </NavLink>
    );
}
