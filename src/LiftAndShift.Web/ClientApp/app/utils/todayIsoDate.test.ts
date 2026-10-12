import { describe, expect, it } from 'vitest'
import { todayIsoDate } from './todayIsoDate'

describe('todayIsoDate', () => {
  it('formats the local date as YYYY-MM-DD with zero-padding', () => {
    expect(todayIsoDate(new Date(2026, 2, 5, 23, 59))).toBe('2026-03-05')
  })

  it('uses the local date rather than the UTC date just after local midnight', () => {
    expect(todayIsoDate(new Date(2026, 9, 12, 0, 30))).toBe('2026-10-12')
  })
})
