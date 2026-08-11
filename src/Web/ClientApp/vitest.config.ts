import { defineVitestConfig } from '@nuxt/test-utils/config'

export default defineVitestConfig({
  test: {
    environment: 'nuxt',
    globals: true,
    include: ['**/*.{test,spec}.ts'],
    exclude: ['node_modules', '.nuxt', '.output', 'e2e'],
  },
})
