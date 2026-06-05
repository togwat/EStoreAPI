import { describe, it, expect } from 'vitest'
import { formatPrice } from '@/lib/formatPrice'

describe('formatPrice', () => {
    it('formats a whole number with two decimal places', () => {
        expect(formatPrice(250)).toBe('$250.00')
    })

    it('formats a decimal with cents padded', () => {
        expect(formatPrice(99.5)).toBe('$99.50')
    })

    it('formats zero', () => {
        expect(formatPrice(0)).toBe('$0.00')
    })

    it('formats a large number with a thousands separator', () => {
        expect(formatPrice(12500)).toBe('$12,500.00')
    })
})
