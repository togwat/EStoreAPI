import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect, vi } from 'vitest'
import { JobCard } from '@/pages/jobs/components/JobCard'
import { JobStatus, type Job } from '@/api/jobs'
import type { Customer } from '@/api/customers'
import type { Device } from '@/api/devices'

const baseJob: Job = {
    jobId: '1',
    customerId: '1',
    deviceId: '1',
    receiveTime: '2024-01-01T08:00:00Z',
    pickupTime: '',
    estimatedPickupTime: '',
    note: '',
    problems: [],
    estimatedPrice: null,
    collectedPrice: null,
    status: JobStatus.InProgress,
}

const customer: Customer = {
    id: '1',
    name: 'John Smith',
    primaryContact: '0211234567',
    phone: '',
    email: '',
    address: '',
}

const device: Device = { id: '1', name: 'iPhone 14', type: 'Smartphone', modelNumber: 'A2649' }

describe('JobCard', () => {
    it('renders the customer name and device name', () => {
        render(<JobCard job={baseJob} customer={customer} device={device} onClick={vi.fn()} />)
        expect(screen.getByText('John Smith')).toBeInTheDocument()
        expect(screen.getByText('iPhone 14')).toBeInTheDocument()
    })

    it('shows collectedPrice when set', () => {
        render(<JobCard job={{ ...baseJob, collectedPrice: 80 }} customer={customer} device={device} onClick={vi.fn()} />)
        expect(screen.getByText('$80.00')).toBeInTheDocument()
    })

    it('shows estimatedPrice when collectedPrice is absent', () => {
        render(<JobCard job={{ ...baseJob, estimatedPrice: 250 }} customer={customer} device={device} onClick={vi.fn()} />)
        expect(screen.getByText('$250.00')).toBeInTheDocument()
    })

    it('shows no price when both prices are absent', () => {
        render(<JobCard job={baseJob} customer={customer} device={device} onClick={vi.fn()} />)
        expect(screen.queryByText(/\$/)).toBeNull()
    })

    it('shows "Picked up" text when pickupTime is set', () => {
        render(<JobCard job={{ ...baseJob, pickupTime: '2024-01-10T14:30:00Z' }} customer={customer} device={device} onClick={vi.fn()} />)
        expect(screen.getByText(/Picked up/)).toBeInTheDocument()
    })

    it('shows "Due" text when only estimatedPickupTime is set', () => {
        render(<JobCard job={{ ...baseJob, estimatedPickupTime: '2024-01-15T17:00:00Z' }} customer={customer} device={device} onClick={vi.fn()} />)
        expect(screen.getByText(/Due/)).toBeInTheDocument()
    })

    it('formats a phone-style primary contact in NZ mobile style', () => {
        render(<JobCard job={baseJob} customer={customer} device={device} onClick={vi.fn()} />)
        expect(screen.getByText('021 123 4567')).toBeInTheDocument()
    })

    it('shows a non-phone primary contact as-is', () => {
        render(<JobCard job={baseJob} customer={{ ...customer, primaryContact: 'john@example.com' }} device={device} onClick={vi.fn()} />)
        expect(screen.getByText('john@example.com')).toBeInTheDocument()
    })

    it('calls onClick when clicked', async () => {
        const onClick = vi.fn()
        render(<JobCard job={baseJob} customer={customer} device={device} onClick={onClick} />)
        await userEvent.click(screen.getByRole('button'))
        expect(onClick).toHaveBeenCalledOnce()
    })

    it('adds accent classes when isSelected is true', () => {
        render(<JobCard job={baseJob} customer={customer} device={device} isSelected onClick={vi.fn()} />)
        const card = screen.getByRole('button')
        expect(card.className).toContain('bg-accent')
    })

    it('does not add accent classes when isSelected is false', () => {
        render(<JobCard job={baseJob} customer={customer} device={device} isSelected={false} onClick={vi.fn()} />)
        const card = screen.getByRole('button')
        expect(card.className).not.toContain('bg-accent')
    })
})
