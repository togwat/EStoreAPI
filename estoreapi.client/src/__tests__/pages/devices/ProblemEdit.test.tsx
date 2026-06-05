import { createRef } from 'react'
import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect } from 'vitest'
import ProblemEdit, { type ProblemEditHandle } from '@/pages/devices/components/ProblemEdit'

// MSW serves fixture problems for device '1':
//   Screen Replacement / $250.00
//   Battery Replacement / $80.00

describe('ProblemEdit (view mode)', () => {
    it('shows problem names and prices as plain text', async () => {
        const ref = createRef<ProblemEditHandle>()
        render(<ProblemEdit ref={ref} deviceId="1" isEditing={false} />)

        expect(await screen.findByText('Screen Replacement')).toBeInTheDocument()
        expect(screen.getByText('$250.00')).toBeInTheDocument()
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
