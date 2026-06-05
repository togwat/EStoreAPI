// converts a UTC datetime string to the local "YYYY-MM-DDTHH:mm" format required by datetime-local inputs
export function toLocalDatetimeInputValue(utcString: string): string {
    const d = new Date(utcString);
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}
