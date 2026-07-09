export default defineNuxtPlugin(() => {
  const theme = localStorage.getItem('picoColorScheme')
  if (theme && theme !== 'auto') {
    document.documentElement.setAttribute('data-theme', theme)
  }
  useThemeStore().init()
})
