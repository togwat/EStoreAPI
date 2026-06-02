export function sortByField<T>(items: T[], field: keyof T, direction: 'asc' | 'desc'): T[] {
    return [...items].sort((a, b) => {
        const cmp = String(a[field] ?? '').localeCompare(String(b[field] ?? ''));
        return direction === 'asc' ? cmp : -cmp;
    });
}