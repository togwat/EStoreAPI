import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect, vi } from 'vitest'
import { DeviceCard } from '@/pages/devices/components/DeviceCard'
import type { Device } from '@/api/devices'

const device: Device = { id: '1', name: 'iPhone 14', type: 'Smartphone' }

describe('DeviceCard', () => {
    it('renders the device name and type', () => {
        render(<DeviceCard device={device} onClick={vi.fn()} />)
        expect(screen.getByText('iPhone 14')).toBeInTheDocument()
        expect(screen.getByText('Smartphone')).toBeInTheDocument()
    })

    it('calls onClick when clicked', async () => {
        const onClick = vi.fn()
        render(<DeviceCard device={device} onClick={onClick} />)
        await userEvent.click(screen.getByRole('button'))
        expect(onClick).toHaveBeenCalledOnce()
    })

    it('calls onClick when Enter is pressed', async () => {
        const onClick = vi.fn()
        render(<DeviceCard device={device} onClick={onClick} />)
        screen.getByRole('button').focus()
        await userEvent.keyboard('{Enter}')
        expect(onClick).toHaveBeenCalledOnce()
    })

    it('adds accent classes when isSelected is true', () => {
        render(<DeviceCard device={device} isSelected onClick={vi.fn()} />)
        const card = screen.getByRole('button')
        expect(card.className).toContain('bg-accent')
    })

    it('does not add accent classes when isSelected is false', () => {
        render(<DeviceCard device={device} isSelected={false} onClick={vi.fn()} />)
        const card = screen.getByRole('button')
        expect(card.className).not.toContain('bg-accent')
    })
})
