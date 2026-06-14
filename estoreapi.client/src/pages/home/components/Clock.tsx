import { useEffect, useState } from "react";

export default function Clock() {
    const [time, setTime] = useState(new Date());

    // updating clock
    useEffect(() => {
        const interval = setInterval(() => {
            setTime(new Date());
        }, 1000);
        return () => clearInterval(interval); // cleanup on unmount
    }, []);

    const dayName = time.toLocaleDateString('en-NZ', { weekday: 'long' }).toUpperCase();
    const day = time.getDate();
    const month = time.toLocaleDateString('en-NZ', { month: 'long' });
    const year = time.getFullYear();  
    const timeStr = time.toLocaleTimeString('en-NZ', {
        hour: 'numeric',
        minute: '2-digit',
        hour12: true
    });

    // rustic-leather theme gets special clock colour, otherwise use primary accent
    return (
        <div className="flex flex-col gap-1 border-b border-border pb-8">
            <span className="text-sm font-bold" style={{ color: 'var(--clock, var(--primary))' }}>{dayName}</span>
            <span className="text-4xl font-bold">{day} {month} <span style={{ color: 'var(--clock, var(--primary))' }}>{year}</span></span>
            <span className="text-muted-foreground font-medium">{timeStr}</span>
        </div>
    )
}