import { expect } from '@playwright/test';
import { BasePage, baseUrl } from './base-page';

export class HistoryPage extends BasePage {
  get pagePath(): string {
    return `${baseUrl()}/history`;
  }

  assertHeading() {
    return expect(this.page.locator('h1')).toHaveText('Workout History');
  }

  assertSessionVisible(exerciseName: string) {
    return expect(
      this.page.locator('article', { hasText: exerciseName }).first()
    ).toBeVisible();
  }

  assertStatusTag(exerciseName: string, status: string) {
    return expect(
      this.page.locator('article', { hasText: exerciseName }).first().locator('.status-tag')
    ).toHaveText(status);
  }

  clickViewFor(exerciseName: string) {
    return this.page
      .locator('article', { hasText: exerciseName })
      .first()
      .locator("a:has-text('View')")
      .click();
  }
}
