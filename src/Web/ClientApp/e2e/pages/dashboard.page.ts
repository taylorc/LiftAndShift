import { expect } from '@playwright/test';
import { BasePage, baseUrl } from './base-page';

export class DashboardPage extends BasePage {
  get pagePath(): string {
    return `${baseUrl()}/dashboard`;
  }

  assertHeading() {
    return expect(this.page.locator('h1')).toHaveText('Dashboard');
  }

  assertPersonalRecordsSectionVisible() {
    return expect(this.page.locator("h2:has-text('Personal Records')")).toBeVisible();
  }

  assertProgrammeCardVisible() {
    return expect(this.page.locator("a[href='/programme']").first()).toBeVisible();
  }

  clickProgrammeLink() {
    return this.page.locator("a[href='/programme']").first().click();
  }

  assertPersonalRecordVisible(exerciseName: string) {
    return expect(
      this.page.locator('table tbody tr', { hasText: exerciseName })
    ).toBeVisible();
  }
}
