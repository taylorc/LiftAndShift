import { test as base, type Page } from '@playwright/test';
import { loginAsAdministrator } from './auth';

export const test = base.extend<{ authenticatedPage: Page }>({
  authenticatedPage: async ({ context }, use) => {
    const page = await loginAsAdministrator(context);
    await use(page);
  },
});

export { expect } from '@playwright/test';
