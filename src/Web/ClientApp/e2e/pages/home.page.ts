import { expect } from '@playwright/test';
import { BasePage, baseUrl } from './base-page';

export class HomePage extends BasePage {
  get pagePath(): string {
    return baseUrl();
  }

  assertHeading(text: string) {
    return expect(this.page.locator('h1')).toHaveText(text);
  }
}
