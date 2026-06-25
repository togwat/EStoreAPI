import { createRef } from 'react'
import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect } from 'vitest'
import ProblemEdit, { type ProblemEditHandle } from '@/pages/devices/components/ProblemEdit'

// MSW serves fixture problems for device '1':
//   Screen Replacement / $250.00 price / $100.00 labour / $50.00 risk / $35.00 parts
//   Battery Replacement / $80.00 price / $40.00 labour / $20.00 risk / $18.00 parts

describe('ProblemEdit (view mode)', () => {
    it('shows problem names, prices, labour prices, risk costs, and parts prices as plain text', async () => {
        const ref = createRef<ProblemEditHandle>()
        render(<ProblemEdit ref={ref} deviceId="1" isEditing={false} />)

        expect(await screen.findByText('Screen Replacement')).toBeInTheDocument()
        expect(screen.getByText('$250.00')).toBeInTheDocument()  // price
        expect(screen.getByText('Labour price')).toBeInTheDocument()
        expect(screen.getByText('$100.00')).toBeInTheDocument()  // labour price
        expect(screen.getByText('Risk cost')).toBeInTheDocument()
        expect(screen.getByText('$50.00')).toBeInTheDocument()   // risk cost
        expect(screen.getByText('Parts price')).toBeInTheDocument()
        expect(screen.getByText('$35.00')).toBeInTheDocument()   // parts price
        expect(screen.queryByRole('textbox')).toBeNull()
    })
})

describe('ProblemEdit (edit mode)', () => {
    it('shows input fields for each problem', async () => {
        const ref = createRef<ProblemEditHandle>()
        render(<ProblemEdit ref={ref} deviceId="1" isEditing={true} />)

        // Wait for data to load — the name input uses the problem name as placeholder
        expect(await screen.findByPlaceholderText('Screen Replacement')).toBeInTheDocument()
        expect(screen.getByPlaceholderText('Battery Replacement')).toBeInTheDocument()

        // price, labour price, risk cost, and parts price inputs use each problem's current formatted value as placeholder
        expect(screen.getByPlaceholderText('$250.00')).toBeInTheDocument()  // price
        expect(screen.getByPlaceholderText('$100.00')).toBeInTheDocument()  // labour price
        expect(screen.getByPlaceholderText('$50.00')).toBeInTheDocument()   // risk cost
        expect(screen.getByPlaceholderText('$35.00')).toBeInTheDocument()   // parts price
    })

    it('edited labour price is exposed through the ref', async () => {
        const ref = createRef<ProblemEditHandle>()
        render(<ProblemEdit ref={ref} deviceId="1" isEditing={true} />)
        await screen.findByPlaceholderText('Screen Replacement')

        // the Screen Replacement labour cell placeholder is its current labour price
        await userEvent.type(screen.getByPlaceholderText('$100.00'), '150')

        const updated = ref.current!.getUpdatedProblems().find(p => p.name === 'Screen Replacement')
        expect(updated!.labourPrice).toBe(150)
    })

    it('edited risk cost is exposed through the ref', async () => {
        const ref = createRef<ProblemEditHandle>()
        render(<ProblemEdit ref={ref} deviceId="1" isEditing={true} />)
        await screen.findByPlaceholderText('Screen Replacement')

        // the Screen Replacement risk cost cell placeholder is its current risk cost
        await userEvent.type(screen.getByPlaceholderText('$50.00'), '75')

        const updated = ref.current!.getUpdatedProblems().find(p => p.name === 'Screen Replacement')
        expect(updated!.riskCost).toBe(75)
    })

    it('edited parts price is exposed through the ref', async () => {
        const ref = createRef<ProblemEditHandle>()
        render(<ProblemEdit ref={ref} deviceId="1" isEditing={true} />)
        await screen.findByPlaceholderText('Screen Replacement')

        // the Screen Replacement parts price cell placeholder is its current parts price
        await userEvent.type(screen.getByPlaceholderText('$35.00'), '40')

        const updated = ref.current!.getUpdatedProblems().find(p => p.name === 'Screen Replacement')
        expect(updated!.partsPrice).toBe(40)
    })

    it('"Add a problem..." button adds a new empty row', async () => {
        const ref = createRef<ProblemEditHandle>()
        render(<ProblemEdit ref={ref} deviceId="1" isEditing={true} />)
        await screen.findByPlaceholderText('Screen Replacement')

        const before = ref.current!.getUpdatedProblems().length
        await userEvent.click(screen.getByRole('button', { name: /add a problem/i }))
        expect(ref.current!.getUpdatedProblems()).toHaveLength(before + 1)
    })

    it('delete button removes the correct row', async () => {
        const ref = createRef<ProblemEditHandle>()
        render(<ProblemEdit ref={ref} deviceId="1" isEditing={true} />)
        await screen.findByPlaceholderText('Screen Replacement')

        // Each data row (skip header at index 0) contains exactly one icon button — the delete button
        const rows = screen.getAllByRole('row')
        const firstDataRow = rows[1]  // Screen Replacement row
        await userEvent.click(within(firstDataRow).getByRole('button'))

        const remaining = ref.current!.getUpdatedProblems()
        expect(remaining).toHaveLength(1)
        expect(remaining[0].name).toBe('Battery Replacement')
    })
})
