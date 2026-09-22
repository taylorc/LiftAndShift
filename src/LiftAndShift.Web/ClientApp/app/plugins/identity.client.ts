// Client-only (localStorage doesn't exist during SSR): loads the previously-selected identity, if
// any, as soon as the app starts on this device.
export default defineNuxtPlugin(() => {
  useIdentityStore().hydrate()
})
