const apiTarget =
  process.env['services__webapi__https__0'] ||
  process.env['services__webapi__http__0'] ||
  ''

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  ssr: true,

  modules: ['@pinia/nuxt'],

  css: ['~/assets/styles.scss'],

  runtimeConfig: {
    apiBaseUrl: apiTarget,
    public: {
      apiBaseUrl: '',
    },
  },

  nitro: {
    routeRules: apiTarget
      ? {
          '/api/**': { proxy: `${apiTarget}/api/**` },
          '/openapi/**': { proxy: `${apiTarget}/openapi/**` },
          '/scalar/**': { proxy: `${apiTarget}/scalar/**` },
          '/weatherforecast': { proxy: `${apiTarget}/weatherforecast` },
          '/WeatherForecast': { proxy: `${apiTarget}/WeatherForecast` },
        }
      : {},
  },


  app: {
    head: {
      link: [
        { rel: 'preconnect', href: 'https://fonts.googleapis.com' },
        { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: '' },
        {
          rel: 'stylesheet',
          href: 'https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=JetBrains+Mono:wght@400;500&family=Outfit:wght@600;700&display=swap',
        },
      ],
    },
  },

  compatibilityDate: '2025-01-01',
})
