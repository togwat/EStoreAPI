import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect } from 'vitest'
import JobsPage from '@/pages/jobs/JobsPage'

// MSW fixture data:
//   Job 1 — John Smith, iPhone 14, Screen Replacement — in progress
//   Job 2 — Jane Doe,   Samsung S22, Battery Replacement — finished

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

    it('filters cards by matching customer name', async () => {
        render(<JobsPage title="Jobs" />)
        await screen.findByText('John Smith')

        await userEvent.type(screen.getByPlaceholderText(/search by customer/i), 'Jane')

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
})
