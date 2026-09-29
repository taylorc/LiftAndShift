import { describe, expect, it } from 'vitest'
import { formatLiftName } from './formatLiftName'

describe('formatLiftName', () => {
  it('splits PascalCase enum identifiers into words', () => {
    expect(formatLiftName('BenchPress')).toBe('Bench Press')
    expect(formatLiftName('LatPulldown')).toBe('Lat Pulldown')
  })

  it('leaves single-word names unchanged', () => {
    expect(formatLiftName('Squat')).toBe('Squat')
  })
})
