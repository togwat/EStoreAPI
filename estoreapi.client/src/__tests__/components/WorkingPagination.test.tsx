import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect, vi } from 'vitest'
import { WorkingPagination } from '@/components/WorkingPagination'

describe('WorkingPagination', () => {
    it('renders nothing when there is only 1 page', () => {
        const { container } = render(
            <WorkingPagination page={1} totalItems={8} itemsPerPage={10} onPageChange={vi.fn()} />
        )
        expect(container.firstChild).toBeNull()
    })

    it('renders nothing when there are 0 items', () => {
        const { container } = render(
            <WorkingPagination page={1} totalItems={0} itemsPerPage={10} onPageChange={vi.fn()} />
        )
        expect(container.firstChild).toBeNull()
    })

    it('disables the Previous button on page 1', () => {
        render(<WorkingPagination page={1} totalItems={30} itemsPerPage={10} onPageChange={vi.fn()} />)
        const prev = screen.getByRole('link', { name: /go to previous page/i })
        expect(prev.className).toContain('pointer-events-none')
    })

    it('disables the Next button on the last page', () => {
        render(<WorkingPagination page={3} totalItems={30} itemsPerPage={10} onPageChange={vi.fn()} />)
        const next = screen.getByRole('link', { name: /go to next page/i })
        expect(next.className).toContain('pointer-events-none')
    })

    it('calls onPageChange with the correct page number when a page is clicked', async () => {
        const onPageChange = vi.fn()
        render(<WorkingPagination page={1} totalItems={30} itemsPerPage={10} onPageChange={onPageChange} />)
        await userEvent.click(screen.getByRole('link', { name: '2' }))
        expect(onPageChange).toHaveBeenCalledWith(2)
    })

    it('shows ellipsis for large page counts', () => {
        render(<WorkingPagination page={5} totalItems={100} itemsPerPage={10} onPageChange={vi.fn()} />)
        // "More pages" is the sr-only label inside PaginationEllipsis
        expect(screen.getAllByText('More pages').length).toBeGreaterThan(0)
    })
})
