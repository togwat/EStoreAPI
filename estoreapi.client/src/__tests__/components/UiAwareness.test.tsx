import { describe, it, expect } from 'vitest'
import { render } from '@testing-library/react'
import { UiAwarenessContext, useUiAwareness, type UiAwarenessRegistry } from '@/components/chat/UiAwareness'

/** A component that reports one UI-awareness line. */
function Reporter({ line }: { line: string | null }) {
    useUiAwareness(line)
    return null
}

/** Render reporters inside a provider backed by the given registry. */
function Tree({ registry, first, second }: { registry: UiAwarenessRegistry; first: string | null; second?: string | null }) {
    return (
        <UiAwarenessContext.Provider value={registry}>
            <Reporter line={first} />
            {second !== undefined && <Reporter line={second} />}
        </UiAwarenessContext.Provider>
    )
}

describe('useUiAwareness', () => {
    it('registers each caller\'s line in mount order', () => {
        const registry: UiAwarenessRegistry = new Map()
        render(<Tree registry={registry} first="Open: job #6" second="Chart: takings" />)
        expect([...registry.values()]).toEqual(['Open: job #6', 'Chart: takings'])
    })

    it('updates a line in place and removes it when set to null', () => {
        const registry: UiAwarenessRegistry = new Map()
        const { rerender } = render(<Tree registry={registry} first="Open: job #6" second="Chart: takings" />)

        rerender(<Tree registry={registry} first="Open: job #9" second="Chart: takings" />)
        expect([...registry.values()]).toEqual(['Open: job #9', 'Chart: takings'])

        rerender(<Tree registry={registry} first={null} second="Chart: takings" />)
        expect([...registry.values()]).toEqual(['Chart: takings'])
    })

    it('removes only the unmounted caller\'s line', () => {
        const registry: UiAwarenessRegistry = new Map()
        const { rerender, unmount } = render(<Tree registry={registry} first="Open: job #6" second="Chart: takings" />)

        rerender(<Tree registry={registry} first="Open: job #6" />)
        expect([...registry.values()]).toEqual(['Open: job #6'])

        unmount()
        expect(registry.size).toBe(0)
    })

    it('is a no-op outside a provider', () => {
        expect(() => render(<Reporter line="Open: job #6" />)).not.toThrow()
    })
})
