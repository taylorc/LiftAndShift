import { defineVitestConfig } from '@nuxt/test-utils/config'

export default defineVitestConfig({
  test: {
    environment: 'nuxt',
    globals: true,
    include: ['**/*.{test,spec}.ts'],
    exclude: ['node_modules', '.nuxt', '.output', 'e2e'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json-summary'],
      exclude: [
        '.nuxt/**',
        'e2e/**',
        'lib/web-api-client.ts',
        'lib/middlewareList.ts',
        'nuxt.config.ts',
        'playwright.config.ts',
        '**/*.config.*',
      ],
    },
  },
})
