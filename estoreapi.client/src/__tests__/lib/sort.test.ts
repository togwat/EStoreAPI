import { describe, it, expect } from 'vitest'
import { sortByField } from '@/lib/sort'

const fruits = [
    { name: 'Banana', rank: 2 },
    { name: 'Apple',  rank: 1 },
    { name: 'Cherry', rank: 3 },
]

describe('sortByField', () => {
    it('sorts ascending by a string field', () => {
        const result = sortByField(fruits, 'name', 'asc')
        expect(result.map(f => f.name)).toEqual(['Apple', 'Banana', 'Cherry'])
    })

    it('sorts descending by a string field', () => {
        const result = sortByField(fruits, 'name', 'desc')
        expect(result.map(f => f.name)).toEqual(['Cherry', 'Banana', 'Apple'])
    })

    it('does not mutate the original array', () => {
        const original = [...fruits]
        sortByField(fruits, 'name', 'asc')
        expect(fruits).toEqual(original)
    })

    it('preserves relative order for items with the same value', () => {
        const tied = [
            { name: 'Apple', id: 1 },
            { name: 'Apple', id: 2 },
            { name: 'Apple', id: 3 },
        ]
        const result = sortByField(tied, 'name', 'asc')
        expect(result.map(t => t.id)).toEqual([1, 2, 3])
    })
})
