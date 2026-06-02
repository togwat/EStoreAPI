import {
    Pagination,
    PaginationContent,
    PaginationEllipsis,
    PaginationItem,
    PaginationLink,
    PaginationNext,
    PaginationPrevious,
} from "@/components/ui/pagination";

/**
 * Setup checklist for adding pagination to a page:
 *
 * 1. Add a `page` state:              const [page, setPage] = useState(1);
 * 2. Reset on filter change:          useEffect(() => { setPage(1); }, [filter]);
 * 3. Slice items to current page:     const paged = items.slice((page - 1) * itemsPerPage, page * itemsPerPage);
 * 4. Render the component:            <WorkingPagination page={page} totalItems={items.length} itemsPerPage={itemsPerPage} onPageChange={setPage} />
 */

// returns page numbers and null as an ellipsis marker
function getPageNumbers(current: number, total: number): (number | null)[] {
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
    if (current <= 4) return [1, 2, 3, 4, 5, null, total];
    if (current >= total - 3) return [1, null, total - 4, total - 3, total - 2, total - 1, total];
    return [1, null, current - 1, current, current + 1, null, total];
}

interface WorkingPaginationProps {
    page: number;
    totalItems: number;
    itemsPerPage: number;
    onPageChange: (page: number) => void;
    window?: number;
    className?: string;
}

export function WorkingPagination({ page, totalItems, itemsPerPage, onPageChange, className }: WorkingPaginationProps) {
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    if (totalPages <= 1) return null;

    return (
        <Pagination className={className}>
            <PaginationContent>
                <PaginationItem>
                    <PaginationPrevious
                        href="#"
                        onClick={(e) => { e.preventDefault(); onPageChange(Math.max(1, page - 1)); }}
                        className={page === 1 ? "pointer-events-none opacity-50" : ""}
                    />
                </PaginationItem>
                {getPageNumbers(page, totalPages).map((n, i) => (
                    <PaginationItem key={i}>
                        {n === null
                            ? <PaginationEllipsis />
                            : <PaginationLink
                                href="#"
                                isActive={page === n}
                                onClick={(e) => { e.preventDefault(); onPageChange(n); }}
                              >
                                {n}
                              </PaginationLink>
                        }
                    </PaginationItem>
                ))}
                <PaginationItem>
                    <PaginationNext
                        href="#"
                        onClick={(e) => { e.preventDefault(); onPageChange(Math.min(totalPages, page + 1)); }}
                        className={page === totalPages ? "pointer-events-none opacity-50" : ""}
                    />
                </PaginationItem>
            </PaginationContent>
        </Pagination>
    );
}
