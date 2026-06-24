import { toLocalDatetimeInputValue } from '@/lib/toLocalDatetime';

// for estimated pickup date, which for now is today + 1
export function getTomorrow(): string {
    const d = new Date();
    d.setDate(d.getDate() + 1);
    return toLocalDatetimeInputValue(d.toISOString());
}
