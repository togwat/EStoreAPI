import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect, vi } from 'vitest'
import { FilterSearch, FilterSelect, FilterSort } from '@/components/Filter'

describe('FilterSearch', () => {
    it('fires onChange with the current input value on each keystroke', async () => {
        const onChange = vi.fn()
        render(<FilterSearch placeholder="Search devices..." onChange={onChange} />)
        await userEvent.type(screen.getByPlaceholderText('Search devices...'), 'mac')
        expect(onChange).toHaveBeenLastCalledWith('mac')
    })
})

describe('FilterSelect', () => {
    it('fires onChange with the selected value', async () => {
        const onChange = vi.fn()
        render(<FilterSelect label="Device type" options={['Smartphone', 'Laptop']} value="all" onChange={onChange} />)
        await userEvent.click(screen.getByRole('combobox'))
        await userEvent.click(screen.getByRole('option', { name: 'Smartphone' }))
        expect(onChange).toHaveBeenCalledWith('Smartphone')
    })
})

describe('FilterSort', () => {
    it('fires onDirectionChange with "desc" when direction is "asc" and toggle is clicked', async () => {
        const onDirectionChange = vi.fn()
        render(
            <FilterSort
                label="Sort by"
                options={['id', 'name', 'type']}
                value="id"
                onChange={vi.fn()}
                direction="asc"
                onDirectionChange={onDirectionChange}
            />
        )
        await userEvent.click(screen.getByRole('button'))
        expect(onDirectionChange).toHaveBeenCalledWith('desc')
    })

    it('fires onDirectionChange with "asc" when direction is "desc" and toggle is clicked', async () => {
        const onDirectionChange = vi.fn()
        render(
            <FilterSort
                label="Sort by"
                options={['id', 'name', 'type']}
                value="id"
                onChange={vi.fn()}
                direction="desc"
                onDirectionChange={onDirectionChange}
            />
        )
        await userEvent.click(screen.getByRole('button'))
        expect(onDirectionChange).toHaveBeenCalledWith('asc')
    })

    it('fires onChange when a different sort field is selected', async () => {
        const onChange = vi.fn()
        render(
            <FilterSort
                label="Sort by"
                options={['id', 'name', 'type']}
                value="id"
                onChange={onChange}
                direction="asc"
                onDirectionChange={vi.fn()}
            />
        )
        await userEvent.click(screen.getByRole('combobox'))
        await userEvent.click(screen.getByRole('option', { name: 'name' }))
        expect(onChange).toHaveBeenCalledWith('name')
    })
})
