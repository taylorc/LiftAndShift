import { expect } from '@playwright/test';
import { BasePage, baseUrl } from './base-page';

export class ProgrammePage extends BasePage {
  get pagePath(): string {
    return `${baseUrl()}/programme`;
  }

  assertHeading() {
    return expect(this.page.locator('h1')).toHaveText('Programme');
  }

  clickStartTemplate(templateName: string) {
    return this.page.locator(`button:has-text('Start ${templateName}')`).click();
  }

  assertActiveProgrammeName(name: string) {
    return expect(
      this.page.locator('article header strong', { hasText: name }).first()
    ).toBeVisible();
  }

  assertNextSessionVisible() {
    return expect(this.page.locator('h2', { hasText: 'Next Session' })).toBeVisible();
  }

  clickStartThisSession() {
    return this.page.locator("button:has-text('Start This Session')").click();
  }
}
