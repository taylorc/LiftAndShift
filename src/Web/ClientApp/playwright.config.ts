import { defineConfig, devices } from '@playwright/test';

const headed = process.argv.includes('--headed') || process.env.PWDEBUG === '1';

export default defineConfig({
  testDir: './e2e/tests',
  globalSetup: './e2e/global-setup.ts',
  fullyParallel: false,
  workers: 1,
  retries: process.env.CI ? 1 : 0,
  reporter: process.env.CI ? [['github'], ['html', { open: 'never' }]] : [['list']],
  // Nuxt runs in dev mode here and compiles each route on first request, so the first
  // navigation to a page can take far longer than the steady-state case.
  timeout: 120_000,
  expect: { timeout: 10_000 },
  use: {
    navigationTimeout: 60_000,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    launchOptions: { slowMo: headed ? 500 : 0 },
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
