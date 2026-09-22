import tailwindcss from '@tailwindcss/vite'

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },
  modules: ['@pinia/nuxt'],
  css: ['~/assets/css/main.css'],
  vite: {
    plugins: [tailwindcss()]
  },
  app: {
    head: {
      link: [
        {
          rel: 'stylesheet',
          href: 'https://fonts.googleapis.com/css2?family=Big+Shoulders+Display:wght@500;700;900&family=IBM+Plex+Sans:wght@400;500;600&display=swap'
        }
      ]
    }
  },
  runtimeConfig: {
    public: {
      // Overridden in dev/prod via NUXT_PUBLIC_API_BASE (Aspire injects the discovered API URL).
      // scripts/dev.mjs makes Node trust the ASP.NET Core dev cert, so HTTPS works locally too.
      apiBase: 'https://localhost:57679'
    }
  }
})
