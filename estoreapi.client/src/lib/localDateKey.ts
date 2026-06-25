// Helpers for keying/parsing dates by local calendar day rather than UTC.
// Day-level keys should route through these helpers to keep a unified timezone
// Not for precise time, or outbound data.
// Use toLocalDatetimeInputValue for more time precision
// Use Date object for date-related arithmetic

// Date -> "YYYY-MM-DD" using local calendar parts
// Use this instead of toISOString().slice(0, 10) for day keys
export function toLocalDateKey(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
}

// "YYYY-MM-DD" -> Date at local midnight (inverse of toLocalDateKey).
// Use this instead of new Date(key) for display formatting
export function parseLocalDateKey(key: string): Date {
    const [year, month, day] = key.split("-").map(Number);
    return new Date(year, month - 1, day);
}