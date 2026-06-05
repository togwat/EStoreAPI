import { describe, it, expect } from 'vitest'
import { toLocalDatetimeInputValue } from '@/lib/toLocalDatetime'

// Tests run with TZ=UTC (set in vitest.config.ts), so UTC time equals local time
// and assertions are deterministic across machines.

describe('toLocalDatetimeInputValue', () => {
    it('returns a string in YYYY-MM-DDTHH:mm format', () => {
        const result = toLocalDatetimeInputValue('2024-06-15T14:30:00Z')
        expect(result).toMatch(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/)
    })

    it('pads single-digit month and minute with a leading zero', () => {
        // January (01), 9th day, 09:05 — all values that would be single-digit unpadded
        const result = toLocalDatetimeInputValue('2024-01-09T09:05:00Z')
        expect(result).toBe('2024-01-09T09:05')
    })

    it('preserves the correct time when converting from UTC (TZ=UTC)', () => {
        const result = toLocalDatetimeInputValue('2024-06-15T14:30:00Z')
        expect(result).toBe('2024-06-15T14:30')
    })
})
