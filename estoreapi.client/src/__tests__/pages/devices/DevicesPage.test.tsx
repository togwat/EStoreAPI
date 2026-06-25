import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect } from 'vitest'
import DevicesPage from '@/pages/devices/DevicesPage'

// MSW fixture data: iPhone 14 / A2882 (Smartphone), Samsung S22 / SM-S901 (Smartphone), MacBook Pro 2023 / A2780 (Laptop)

describe('DevicesPage', () => {
    it('renders device cards from API data', async () => {
        render(<DevicesPage title="Devices" />)
        expect(await screen.findByText('iPhone 14')).toBeInTheDocument()
        expect(screen.getByText('Samsung S22')).toBeInTheDocument()
        expect(screen.getByText('MacBook Pro 2023')).toBeInTheDocument()
    })

    it('filters device cards by search query', async () => {
        render(<DevicesPage title="Devices" />)
        await screen.findByText('iPhone 14')

        await userEvent.type(screen.getByPlaceholderText('Search devices...'), 'iphone')

        expect(screen.getByText('iPhone 14')).toBeInTheDocument()
        expect(screen.queryByText('Samsung S22')).not.toBeInTheDocument()
        expect(screen.queryByText('MacBook Pro 2023')).not.toBeInTheDocument()
    })

    it('filters device cards by model number', async () => {
        render(<DevicesPage title="Devices" />)
        await screen.findByText('iPhone 14')

        // 'A2882' is the iPhone 14's model number; matching mirrors the backend SearchDevicesAsync
        await userEvent.type(screen.getByPlaceholderText('Search devices...'), 'a2882')

        expect(screen.getByText('iPhone 14')).toBeInTheDocument()
        expect(screen.queryByText('Samsung S22')).not.toBeInTheDocument()
        expect(screen.queryByText('MacBook Pro 2023')).not.toBeInTheDocument()
    })

    it('clicking a device card opens the panel and loads its problems', async () => {
        render(<DevicesPage title="Devices" />)
        await screen.findByText('iPhone 14')

        await userEvent.click(screen.getByRole('button', { name: /iPhone 14/i }))

        // Problems for iPhone 14 (device id '1') are Screen Replacement and Battery Replacement
        expect(await screen.findByText('Screen Replacement')).toBeInTheDocument()
        expect(screen.getByText('Battery Replacement')).toBeInTheDocument()

        // The price, labour price, risk cost, and parts price columns render their formatted values
        expect(screen.getByText('Price')).toBeInTheDocument()
        expect(screen.getByText('Labour price')).toBeInTheDocument()
        expect(screen.getByText('Risk cost')).toBeInTheDocument()
        expect(screen.getByText('Parts price')).toBeInTheDocument()
        expect(screen.getByText('$250.00')).toBeInTheDocument()  // Screen Replacement price
        expect(screen.getByText('$100.00')).toBeInTheDocument()  // Screen Replacement labour price
        expect(screen.getByText('$50.00')).toBeInTheDocument()   // Screen Replacement risk cost
        expect(screen.getByText('$35.00')).toBeInTheDocument()   // Screen Replacement parts price
    })

    it('clicking Add device opens the panel in edit mode', async () => {
        render(<DevicesPage title="Devices" />)
        await screen.findByText('iPhone 14')

        await userEvent.click(screen.getByRole('button', { name: /add device/i }))

        // Edit mode shows Confirm and Cancel buttons (only rendered when isEditing=true)
        expect(await screen.findByRole('button', { name: 'Confirm' })).toBeInTheDocument()
        expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument()
    })
})
