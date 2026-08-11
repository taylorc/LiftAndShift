import type { Page } from '@playwright/test';

export function baseUrl(): string {
  const url = process.env.E2E_BASE_URL;
  if (!url) {
    throw new Error('E2E_BASE_URL is not set - did global setup run?');
  }
  return url.replace(/\/$/, '');
}

export abstract class BasePage {
  constructor(protected readonly page: Page) {}

  abstract get pagePath(): string;

  async goto(): Promise<void> {
    await this.page.goto(this.pagePath);

    // Nuxt server-renders the initial HTML, but client-side hydration finishes slightly
    // later. Filling a form before hydration completes can have Vue overwrite the typed
    // value once it reconciles the input against its (still empty) reactive state.
    await this.page.waitForLoadState('networkidle');
  }
}
