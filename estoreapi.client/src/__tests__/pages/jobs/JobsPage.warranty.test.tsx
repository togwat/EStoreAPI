import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect } from 'vitest'
import { http, HttpResponse } from 'msw'
import JobsPage from '@/pages/jobs/JobsPage'
import { server } from '../../mocks/server'

// Backend (pre-mapping) job shape, mirroring the OutJobDTO the API returns.
// customerId 1 -> John Smith and deviceId 1 -> iPhone 14 both come from the default fixtures,
// so the panel's customer/device lookups resolve.
type RawJob = {
    jobId: number
    customerId: number
    deviceId: number
    receiveTime: string
    pickupTime: string | null
    estimatedPickupTime: string | null
    note: string | null
    problems: { problemId: number; problemName: string; deviceId: number; price: number; labourPrice: number; riskCost: number }[]
    estimatedPrice: number | null
    collectedPrice: number | null
    isFinished: boolean
    warrantyOfJobId: number | null
}

function makeJob(overrides: Partial<RawJob> = {}): RawJob {
    return {
        jobId: 10,
        customerId: 1,
        deviceId: 1,
        receiveTime: '2024-01-01T08:00:00Z',
        pickupTime: null,
        estimatedPickupTime: null,
        note: '',
        problems: [],
        estimatedPrice: null,
        collectedPrice: null,
        isFinished: false,
        warrantyOfJobId: null,
        ...overrides,
    }
}

// Serve a controlled set of jobs, then render the page and wait for the first card.
async function renderWithJobs(jobs: RawJob[]) {
    server.use(http.get('/api/Jobs', () => HttpResponse.json(jobs)))
    render(<JobsPage title="Jobs" />)
    await screen.findByText('John Smith')
}

// Matches the deepest element whose combined text equals `text`, tolerating labels split
// across child elements — the warranty label renders as "Warranty of " + a styled "#5" span.
function fullText(text: string) {
    const norm = (s: string | null | undefined) => s?.replace(/\s+/g, ' ').trim()
    return (_content: string, el: Element | null) =>
        norm(el?.textContent) === text && Array.from(el?.children ?? []).every(c => norm(c.textContent) !== text)
}

// Open the (single) card's detail panel.
async function openPanel() {
    await userEvent.click(screen.getAllByRole('button', { name: /John Smith/i })[0])
    await waitFor(() => expect(document.querySelector('.lucide-x')).not.toBeNull())
}

describe('JobsPage — warranty job display', () => {
    // Job card's price label changes to 'Warranty of #{warrantyOfJobId}' for a warranty job
    it("shows 'Warranty of #<id>' in place of the price on the card", async () => {
        await renderWithJobs([makeJob({ jobId: 10, warrantyOfJobId: 5, estimatedPrice: 250 })])

        expect(screen.getByText(fullText('Warranty of #5'))).toBeInTheDocument()
        // the warranty link replaces the price, so the price is not shown
        expect(screen.queryByText('$250.00')).not.toBeInTheDocument()
    })

    // The opened panel renders 'Warranty of #{warrantyOfJobId}' for a warranty job
    it("shows 'Warranty of #<id>' inside the opened panel", async () => {
        await renderWithJobs([makeJob({ jobId: 10, warrantyOfJobId: 5 })])
        await openPanel()

        // appears on the card and again inside the panel
        expect(screen.getAllByText(fullText('Warranty of #5')).length).toBeGreaterThanOrEqual(2)
    })

    // Problems do not render in the panel if job is a warranty job
    it('does not render the problems section for a warranty job', async () => {
        await renderWithJobs([makeJob({ jobId: 10, warrantyOfJobId: 5 })])
        await openPanel()

        expect(screen.queryByText('PROBLEMS')).not.toBeInTheDocument()
    })

    // Prices do not render in the panel if job is a warranty job
    it('does not render prices for a warranty job', async () => {
        await renderWithJobs([makeJob({
            jobId: 10,
            warrantyOfJobId: 5,
            estimatedPrice: 199,
            collectedPrice: 88,
        })])
        await openPanel()

        expect(screen.queryByText('EST. PRICE')).not.toBeInTheDocument()
        expect(screen.queryByText('$199.00')).not.toBeInTheDocument()
        expect(screen.queryByText('$88.00')).not.toBeInTheDocument()
    })
})

describe('JobsPage — Create warranty job button', () => {
    // Button shows only if the job is finished and the panel is not in editing mode
    it('shows the button for a finished job in view mode, and hides it once editing starts', async () => {
        await renderWithJobs([makeJob({ jobId: 10, isFinished: true, pickupTime: '2024-02-01T10:00:00Z' })])
        await openPanel()

        // finished + view mode -> visible
        expect(screen.getByRole('button', { name: /create warranty job/i })).toBeInTheDocument()

        // enter edit mode via the pencil -> hidden
        await userEvent.click(document.querySelector('.lucide-pencil')!.closest('button')!)
        expect(screen.queryByRole('button', { name: /create warranty job/i })).not.toBeInTheDocument()
    })

    it('hides the button for an in-progress job', async () => {
        await renderWithJobs([makeJob({ jobId: 10, isFinished: false })])
        await openPanel()

        expect(screen.queryByRole('button', { name: /create warranty job/i })).not.toBeInTheDocument()
    })
})

describe('JobsPage — AddWarrantyPanel', () => {
    // helper: open a finished job's panel and switch to the AddWarrantyPanel
    async function openAddWarranty(pickupTime = '2024-02-01T10:00:00Z') {
        await renderWithJobs([makeJob({ jobId: 10, isFinished: true, pickupTime })])
        await openPanel()
        await userEvent.click(screen.getByRole('button', { name: /create warranty job/i }))
        await screen.findByText(/add warranty for/i)
    }

    // Clicking create warranty job button changes the panel to AddWarrantyPanel
    it('swaps the panel to AddWarrantyPanel when the button is clicked', async () => {
        await openAddWarranty()
        expect(screen.getByText(/add warranty for/i)).toBeInTheDocument()
    })

    // Cancel reverts back to the previous (detail) panel
    it('returns to the detail panel when Cancel is clicked', async () => {
        await openAddWarranty()

        await userEvent.click(screen.getByRole('button', { name: /cancel/i }))

        expect(screen.getByText('CUSTOMER')).toBeInTheDocument()       // detail panel is back
        expect(screen.queryByText(/add warranty for/i)).not.toBeInTheDocument()
    })

    // Clicking X closes the panel entirely
    it('closes the panel entirely when X is clicked', async () => {
        await openAddWarranty()

        await userEvent.click(document.querySelector('.lucide-x')!.closest('button')!)

        expect(screen.queryByText(/add warranty for/i)).not.toBeInTheDocument()
        expect(screen.queryByText('CUSTOMER')).not.toBeInTheDocument() // no detail panel either
    })

    // Alert with title 'Warranty expired' if the original job's pickup was over 3 months ago
    it("shows a 'Warranty expired' alert when the original pickup was over 3 months ago", async () => {
        const sixMonthsAgo = new Date(Date.now() - 1000 * 60 * 60 * 24 * 180).toISOString()
        await openAddWarranty(sixMonthsAgo)

        expect(screen.getByText(/warranty expired/i)).toBeInTheDocument()
    })

    it('shows no expired alert when the original pickup was recent', async () => {
        const lastWeek = new Date(Date.now() - 1000 * 60 * 60 * 24 * 7).toISOString()
        await openAddWarranty(lastWeek)

        expect(screen.queryByText(/warranty expired/i)).not.toBeInTheDocument()
    })
})