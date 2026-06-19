export function sortByField<T>(items: T[], field: keyof T, direction: 'asc' | 'desc', numeric = false): T[] {
    return [...items].sort((a, b) => {
        const cmp = String(a[field] ?? '').localeCompare(String(b[field] ?? ''), undefined, { numeric });
        return direction === 'asc' ? cmp : -cmp;
    });
}