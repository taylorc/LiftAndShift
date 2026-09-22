// Which Family Member this device is currently acting as. Not authentication (see ADR 0001) - just
// a client-side selection that persists on this device until someone switches. localStorage access is
// wrapped in try/catch: it can throw or return nothing in a private window, with blocked/cleared site
// data, or during SSR (where it doesn't exist at all - hydration happens client-only, see
// plugins/identity.client.ts).
const STORAGE_KEY = 'lift-and-shift:identity'

interface StoredIdentity {
  familyMemberId: number
  name: string
}

export const useIdentityStore = defineStore('identity', () => {
  const familyMemberId = ref<number | null>(null)
  const name = ref<string | null>(null)

  function select(id: number, memberName: string) {
    familyMemberId.value = id
    name.value = memberName
    persist({ familyMemberId: id, name: memberName })
  }

  function clear() {
    familyMemberId.value = null
    name.value = null
    persist(null)
  }

  function hydrate() {
    const stored = read()
    if (stored) {
      familyMemberId.value = stored.familyMemberId
      name.value = stored.name
    }
  }

  function persist(value: StoredIdentity | null) {
    try {
      if (value) {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(value))
      } else {
        localStorage.removeItem(STORAGE_KEY)
      }
    } catch {
      // Best-effort only; the in-memory state above still works for the rest of this session.
    }
  }

  function read(): StoredIdentity | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY)
      return raw ? (JSON.parse(raw) as StoredIdentity) : null
    } catch {
      return null
    }
  }

  return { familyMemberId, name, select, clear, hydrate }
})
