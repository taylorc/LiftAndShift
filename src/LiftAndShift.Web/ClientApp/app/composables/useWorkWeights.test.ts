// @vitest-environment nuxt
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'

const { apiMock } = vi.hoisted(() => ({ apiMock: vi.fn() }))

mockNuxtImport('useApi', () => () => apiMock)
mockNuxtImport('useIdentityStore', () => () => ({ familyMemberId: 7 }))

describe('useWorkWeights', () => {
  beforeEach(() => {
    apiMock.mockReset()
  })

  it("fetches each lift's Work Weight for the selected Family Member", async () => {
    apiMock.mockImplementation(async (url: string) => ({
      weightKg: url.endsWith('/Squat') ? 60 : 40
    }))

    const result = await useWorkWeights().fetchWorkWeights(['Squat', 'Press'])

    expect(result).toEqual({ Squat: 60, Press: 40 })
    expect(apiMock).toHaveBeenCalledWith('/FamilyMembers/7/WorkWeights/Squat')
    expect(apiMock).toHaveBeenCalledWith('/FamilyMembers/7/WorkWeights/Press')
  })

  it('treats a lift the API 404s on (not yet Ramped) as null rather than failing', async () => {
    apiMock.mockImplementation(async (url: string) => {
      if (url.endsWith('/Row')) throw new Error('404 Not Found')
      return { weightKg: 60 }
    })

    const result = await useWorkWeights().fetchWorkWeights(['Squat', 'Row'])

    expect(result).toEqual({ Squat: 60, Row: null })
  })
})
