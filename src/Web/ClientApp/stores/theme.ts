import { defineStore } from 'pinia'

const STORAGE_KEY = 'picoColorScheme'

export const useThemeStore = defineStore('theme', {
  state: () => ({
    theme: 'auto' as 'auto' | 'light' | 'dark',
  }),

  actions: {
    init() {
      if (import.meta.client) {
        this.theme = (localStorage.getItem(STORAGE_KEY) as 'auto' | 'light' | 'dark') || 'auto'
      }
    },

    setTheme(value: 'auto' | 'light' | 'dark') {
      this.theme = value
      if (import.meta.client) {
        localStorage.setItem(STORAGE_KEY, value)
        if (value === 'auto') {
          document.documentElement.removeAttribute('data-theme')
        } else {
          document.documentElement.setAttribute('data-theme', value)
        }
      }
    },

    cycleTheme() {
      const next = { auto: 'light', light: 'dark', dark: 'auto' } as const
      this.setTheme(next[this.theme])
    },
  },
})
