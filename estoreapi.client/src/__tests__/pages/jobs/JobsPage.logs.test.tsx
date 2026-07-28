import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect } from 'vitest'
import JobsPage from '@/pages/jobs/JobsPage'

// Render the page and wait for the first card.
async function renderPage() {
    render(<JobsPage title="Jobs" />)
    await screen.findByText('John Smith')
}

// Open a customer's edit panel.
async function openPanel(customer: RegExp) {
    // substring/case-insensitive: the card's accessible name is more than just the customer name
    await userEvent.click(screen.getAllByRole('button', { name: new RegExp(customer, 'i') })[0])
    await waitFor(() => expect(document.querySelector('.lucide-x')).not.toBeNull())
}

// Open the detail panel, then switch to the log panel via the clipboard-clock button.
async function openLogs(customer: RegExp) {
    await openPanel(customer)
    await userEvent.click(document.querySelector('.lucide-clipboard-clock')!.closest('button')!)
    await screen.findByText(/log of/i)
}

describe('JobsPage — JobLogPanel', () => {
    // The ClipboardClock button swaps the detail panel for the log panel
    it('switches to the log panel when the clipboard-clock button is clicked', async () => {
        await renderPage()
        await openPanel(/John Smith/i)

        expect(screen.getByText('CUSTOMER')).toBeInTheDocument()    // detail panel first

        await userEvent.click(document.querySelector('.lucide-clipboard-clock')!.closest('button')!)

        expect(await screen.findByText(/log of/i)).toBeInTheDocument()   // log panel shown
        expect(screen.queryByText('CUSTOMER')).not.toBeInTheDocument()  // detail panel gone
    })

    // The back button in the log panel returns to the edit panel
    it('returns to the detail panel when the back button is clicked', async () => {
        await renderPage()
        await openLogs(/John Smith/i)

        await userEvent.click(document.querySelector('.lucide-chevron-left')!.closest('button')!)

        expect(screen.getByText('CUSTOMER')).toBeInTheDocument()        // detail panel is back
        expect(screen.queryByText(/log of/i)).not.toBeInTheDocument()   // log panel gone
    })

    // Only the selected job's logs are rendered
    it('renders the log entries of the selected job', async () => {
        await renderPage()
        await openLogs(/John Smith/i)

        expect(screen.getAllByText('Status change:')).toHaveLength(2)
        expect(screen.getByText('booked in — cracked screen')).toBeInTheDocument()
        expect(screen.getByText('$100.00')).toBeInTheDocument()
        // Jane's log must not appear in John's panel
        expect(screen.queryByText('JANE ONLY LOG')).not.toBeInTheDocument()
    })

    // Each entry renders exactly the fields that are non-null on its log
    it('renders only the non-null fields of each log entry', async () => {
        await renderPage()
        await openLogs(/John Smith/i)

        expect(screen.getAllByText('Status change:')).toHaveLength(2)
        expect(screen.getAllByText('Note change:')).toHaveLength(1)
        expect(screen.getAllByText('Money change:')).toHaveLength(1)
    })

    // a refund renders as a styled negative and that a null note produces no note line for that entry
    it('renders negative money and no note for a status + money entry', async () => {
        await renderPage()
        await openLogs(/Jane Doe/i)

        const money = screen.getByText('-$80.00')
        expect(money).toBeInTheDocument()
        expect(money).toHaveClass('text-destructive')

        expect(screen.getAllByText('Status change:')).toHaveLength(2)
        expect(screen.getAllByText('Money change:')).toHaveLength(1)
        expect(screen.getAllByText('Note change:')).toHaveLength(1)
    })
})
