import { describe, expect, it } from 'vitest'
import { formatDate } from './formatDate'

describe('formatDate', () => {
  it('formats an ISO date as day, short month, year', () => {
    expect(formatDate('2026-03-05')).toBe(new Date('2026-03-05').toLocaleDateString(undefined, {
      day: 'numeric',
      month: 'short',
      year: 'numeric'
    }))
  })
})
