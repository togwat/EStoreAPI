import { AlertCircleIcon, CheckCircle2Icon } from 'lucide-react';
import { toast as _toast } from 'react-toastify';

const baseOptions = {
    position: "top-center" as const,
    autoClose: false as const,
    className: "border border-border bg-card!"
}

export const toast = {
    error: (title: string, message?: string) => {
        _toast(
            <div>
                <p className="font-semibold text-destructive">{title}</p>
                {message && <p className="text-sm text-destructive">{message}</p>}
            </div>,
            {
                ...baseOptions,
                icon: <AlertCircleIcon className="text-destructive" />
            }
        )
    },
    success: (title: string, message?: string) => {
        _toast(
            <div>
                <p className="font-semibold text-green-600">{title}</p>
                {message && <p className="text-sm text-green-600">{message}</p>}
            </div>,
            {
                ...baseOptions,
                icon: <CheckCircle2Icon className="text-green-600" />,
                autoClose: 4000,
                pauseOnHover: true,
                hideProgressBar: true
            }
        )
    }
}