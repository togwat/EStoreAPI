import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect } from 'vitest'
import JobsPage from '@/pages/jobs/JobsPage'

// MSW fixture data:
//   Job 1 — John Smith (contact 0211234567),      iPhone 14,   Screen Replacement — in progress
//   Job 2 — Jane Doe   (contact jane@example.com), Samsung S22, Battery Replacement — finished
//   Job 3 — Smithy Jonny  (contact 0273334444),       iPhone 14,   Screen Replacement — cancelled
//   Job 4 — Jinny Doey  (contact 0274445555),       iPhone 14,   Screen Replacement — refunded

// Scope queries to one section: the div wrapping the header row and its cards. Asserting
// within it proves which section a card renders under, not merely that it rendered at all.
function section(header: string) {
    return within(screen.getByText(header).closest('div')!.parentElement!)
}

describe('JobsPage', () => {
    it('renders IN PROGRESS and FINISHED sections with the correct cards', async () => {
        render(<JobsPage title="Jobs" />)
        expect(await screen.findByText('IN PROGRESS')).toBeInTheDocument()
        expect(await screen.findByText('FINISHED')).toBeInTheDocument()
        expect(screen.getByText('John Smith')).toBeInTheDocument()
        expect(screen.getByText('Jane Doe')).toBeInTheDocument()
    })

    it('hides the FINISHED section when the "In progress" filter is selected', async () => {
        render(<JobsPage title="Jobs" />)
        await screen.findByText('IN PROGRESS')

        await userEvent.click(screen.getByRole('combobox'))
        await userEvent.click(screen.getByRole('option', { name: 'In progress' }))

        expect(screen.queryByText('FINISHED')).not.toBeInTheDocument()
        expect(screen.getByText('IN PROGRESS')).toBeInTheDocument()
    })

    it('hides the IN PROGRESS section when the "Finished" filter is selected', async () => {
        render(<JobsPage title="Jobs" />)
        await screen.findByText('FINISHED')

        await userEvent.click(screen.getByRole('combobox'))
        await userEvent.click(screen.getByRole('option', { name: 'Finished' }))

        expect(screen.queryByText('IN PROGRESS')).not.toBeInTheDocument()
        expect(screen.getByText('FINISHED')).toBeInTheDocument()
    })

    it('filters cards by matching customer name or device name', async () => {
        render(<JobsPage title="Jobs" />)
        await screen.findByText('John Smith')

        const search = screen.getByPlaceholderText(/search jobs/i)

        // match by customer name
        await userEvent.type(search, 'Jane')
        expect(screen.queryByText('John Smith')).not.toBeInTheDocument()
        expect(screen.getByText('Jane Doe')).toBeInTheDocument()

        // match by device name — Samsung S22 belongs to Jane Doe
        await userEvent.clear(search)
        await userEvent.type(search, 'Samsung')
        expect(screen.queryByText('John Smith')).not.toBeInTheDocument()
        expect(screen.getByText('Jane Doe')).toBeInTheDocument()
    })

    it('filters cards by matching the customer primary contact', async () => {
        render(<JobsPage title="Jobs" />)
        await screen.findByText('John Smith')

        const search = screen.getByPlaceholderText(/search jobs/i)

        // phone-style primary contact — John's is 0211234567
        await userEvent.type(search, '021123')
        expect(screen.getByText('John Smith')).toBeInTheDocument()
        expect(screen.queryByText('Jane Doe')).not.toBeInTheDocument()

        // non-phone primary contact — Jane's is jane@example.com
        await userEvent.clear(search)
        await userEvent.type(search, 'jane@')
        expect(screen.queryByText('John Smith')).not.toBeInTheDocument()
        expect(screen.getByText('Jane Doe')).toBeInTheDocument()
    })

    it('clicking a card opens the detail panel showing the customer section', async () => {
        render(<JobsPage title="Jobs" />)
        await screen.findByText('John Smith')

        // The card and panel header both say "John Smith" — click the card role button
        const cards = screen.getAllByRole('button', { name: /John Smith/i })
        await userEvent.click(cards[0])

        // The panel shows a "CUSTOMER" section header — only rendered inside the panel
        expect(await screen.findByText('CUSTOMER')).toBeInTheDocument()
    })

    it('groups cancelled and refunded jobs under the FINISHED section', async () => {
        render(<JobsPage title="Jobs" />)
        await screen.findByText('FINISHED')

        const finished = section('FINISHED')
        expect(finished.getByText('#2')).toBeInTheDocument()   // finished
        expect(finished.getByText('#3')).toBeInTheDocument()   // cancelled
        expect(finished.getByText('#4')).toBeInTheDocument()   // refunded

        // only the in progress job sits in the section above
        const inProgress = section('IN PROGRESS')
        expect(inProgress.getByText('#1')).toBeInTheDocument()
        expect(inProgress.queryByText('#3')).not.toBeInTheDocument()
        expect(inProgress.queryByText('#4')).not.toBeInTheDocument()
    })

    it('shows only the cancelled job when the "Cancelled" filter is selected', async () => {
        render(<JobsPage title="Jobs" />)
        await screen.findByText('FINISHED')

        await userEvent.click(screen.getByRole('combobox'))
        await userEvent.click(screen.getByRole('option', { name: 'Cancelled' }))

        expect(screen.getByText('#3')).toBeInTheDocument()
        expect(screen.queryByText('#1')).not.toBeInTheDocument()
        expect(screen.queryByText('#2')).not.toBeInTheDocument()
        expect(screen.queryByText('#4')).not.toBeInTheDocument()
    })

    it('shows only the refunded job when the "Refunded" filter is selected', async () => {
        render(<JobsPage title="Jobs" />)
        await screen.findByText('FINISHED')

        await userEvent.click(screen.getByRole('combobox'))
        await userEvent.click(screen.getByRole('option', { name: 'Refunded' }))

        expect(screen.getByText('#4')).toBeInTheDocument()
        expect(screen.queryByText('#1')).not.toBeInTheDocument()
        expect(screen.queryByText('#2')).not.toBeInTheDocument()
        expect(screen.queryByText('#3')).not.toBeInTheDocument()
    })
})
