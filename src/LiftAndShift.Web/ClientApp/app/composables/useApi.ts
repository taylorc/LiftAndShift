// Thin wrapper around $fetch pointed at the LiftAndShift API. Works both during SSR (direct
// server-to-server call, no CORS involved) and client-side (browser fetch, CORS-enabled on the API).
export function useApi() {
  const config = useRuntimeConfig()

  return $fetch.create({
    baseURL: config.public.apiBase
  })
}
