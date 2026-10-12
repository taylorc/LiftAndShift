// Which Family Member this device is currently acting as. Not authentication (see ADR 0001) - just
// a selection that persists on this device until someone switches. Kept in a cookie rather than
// localStorage so SSR sees it too: otherwise every server render thinks nobody is selected, which
// bounces direct loads of identity-guarded pages back to `/` and makes the header hydrate differently
// from how the server rendered it.
const COOKIE_NAME = 'lift-and-shift-identity'
const ONE_YEAR_SECONDS = 60 * 60 * 24 * 365

interface StoredIdentity {
  familyMemberId: number
  name: string
}

export const useIdentityStore = defineStore('identity', () => {
  const cookie = useCookie<StoredIdentity | null>(COOKIE_NAME, {
    default: () => null,
    maxAge: ONE_YEAR_SECONDS,
    sameSite: 'lax'
  })

  const familyMemberId = computed(() => cookie.value?.familyMemberId ?? null)
  const name = computed(() => cookie.value?.name ?? null)

  function select(id: number, memberName: string) {
    cookie.value = { familyMemberId: id, name: memberName }
  }

  function clear() {
    cookie.value = null
  }

  return { familyMemberId, name, select, clear }
})
