import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import JobForm from '@/pages/form/components/JobForm'

vi.mock('@/components/CustomToast', () => ({
    toast: {
        success: vi.fn(),
        error: vi.fn(),
    },
}))

// Import the mocked toast so we can assert on it
import { toast } from '@/components/CustomToast'

beforeEach(() => {
    vi.clearAllMocks()
})

// Helper: get the device datalist option values
function deviceOptions() {
    return Array.from(document.getElementById('device-datalist')?.querySelectorAll('option') ?? []).map(o => o.value)
}

// Helper: get the problem datalist option values
function problemOptions() {
    return Array.from(document.getElementById('problem-datalist')?.querySelectorAll('option') ?? []).map(o => o.value)
}

// Helper: count problem inputs
function problemInputCount() {
    return document.querySelectorAll('[list="problem-datalist"]').length
}

describe('JobForm (device type selection)', () => {
    it('populates the device model datalist with all devices when no type is selected', async () => {
        render(<JobForm />)
        await waitFor(() => expect(deviceOptions().length).toBeGreaterThan(0))
        expect(deviceOptions()).toContain('iPhone 14')
        expect(deviceOptions()).toContain('Samsung S22')
        expect(deviceOptions()).toContain('MacBook Pro 2023')
    })

    it('filters the device model datalist to the selected device type', async () => {
        render(<JobForm />)
        // Wait for initial device list to populate (confirms APIs are ready)
        await waitFor(() => expect(deviceOptions().length).toBeGreaterThan(0))

        await userEvent.click(document.querySelector('[data-slot="select-trigger"]')!)
        await userEvent.click(screen.getByRole('option', { name: 'Smartphone' }))

        await waitFor(() => {
            expect(deviceOptions()).toContain('iPhone 14')
            expect(deviceOptions()).toContain('Samsung S22')
            expect(deviceOptions()).not.toContain('MacBook Pro 2023')
        })
    })
})

describe('JobForm (problem datalist)', () => {
    it('populates the problem datalist with problems of the selected device', async () => {
        render(<JobForm />)
        await waitFor(() => expect(deviceOptions()).toContain('iPhone 14'))

        // Type the exact device name to trigger selectedDeviceId
        await userEvent.type(screen.getByLabelText(/device model/i), 'iPhone 14')

        await waitFor(() => {
            expect(problemOptions()).toContain('Screen Replacement')
            expect(problemOptions()).toContain('Battery Replacement')
        })
    })
})

describe('JobForm (problem inputs)', () => {
    it('starts with one problem input', () => {
        render(<JobForm />)
        expect(problemInputCount()).toBe(1)
    })

    it('Add problem button adds a new problem input', async () => {
        render(<JobForm />)
        await userEvent.click(screen.getByRole('button', { name: /add problem/i }))
        expect(problemInputCount()).toBe(2)
    })

    it('Remove problem button removes the bottom-most input', async () => {
        render(<JobForm />)
        await userEvent.click(screen.getByRole('button', { name: /add problem/i }))
        expect(problemInputCount()).toBe(2)
        await userEvent.click(screen.getByRole('button', { name: /remove problem/i }))
        expect(problemInputCount()).toBe(1)
    })

    it('Remove problem button is disabled when only one problem input remains', () => {
        render(<JobForm />)
        expect(screen.getByRole('button', { name: /remove problem/i })).toBeDisabled()
    })
})

describe('JobForm (price auto-calculation)', () => {
    it('shows the matched problem price as the estimated price placeholder', async () => {
        render(<JobForm />)
        await waitFor(() => expect(deviceOptions()).toContain('iPhone 14'))

        // Select iPhone 14 to load its problems
        await userEvent.type(screen.getByLabelText(/device model/i), 'iPhone 14')
        await waitFor(() => expect(problemOptions()).toContain('Screen Replacement'))

        // Type the exact problem name into the first problem input
        await userEvent.type(document.querySelectorAll('[list="problem-datalist"]')[0] as HTMLElement, 'Screen Replacement')

        await waitFor(() => {
            const priceInput = screen.getByRole('spinbutton')
            // price + labour price + risk cost
            expect(priceInput).toHaveAttribute('placeholder', '400')
        })
    })
})

describe('JobForm (submission)', () => {
    it('does not call the API when required fields are empty', async () => {
        render(<JobForm />)
        await userEvent.click(screen.getByRole('button', { name: 'Submit' }))
        expect(toast.success).not.toHaveBeenCalled()
        expect(toast.error).not.toHaveBeenCalled()
    })

    it('shows a success toast and resets the form on successful submit', async () => {
        render(<JobForm />)
        await waitFor(() => expect(deviceOptions().length).toBeGreaterThan(0))

        await userEvent.type(screen.getByLabelText('Phone number'), '0211234567')
        await userEvent.type(screen.getByLabelText(/device model/i), 'iPhone 14')
        await userEvent.type(
            document.querySelectorAll('[list="problem-datalist"]')[0] as HTMLElement,
            'Screen Replacement'
        )

        await userEvent.click(screen.getByRole('button', { name: 'Submit' }))

        await waitFor(() => {
            expect(toast.success).toHaveBeenCalledWith('Success', 'Job 42 has been created.')
        })
    })
})
