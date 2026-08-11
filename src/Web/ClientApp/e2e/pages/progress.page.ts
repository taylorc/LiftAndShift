import { expect } from '@playwright/test';
import { BasePage, baseUrl } from './base-page';

export class ProgressPage extends BasePage {
  get pagePath(): string {
    return `${baseUrl()}/progress`;
  }

  assertHeading() {
    return expect(this.page.locator('h1')).toHaveText('Progress');
  }

  async selectExerciseByName(name: string) {
    const select = this.page.locator('select').first();
    const value = await select.locator('option', { hasText: name }).first().getAttribute('value');
    await select.selectOption(value);
  }

  assertTableRowVisible() {
    return expect(this.page.locator('table tbody tr').first()).toBeVisible();
  }

  assertNoDataMessageVisible() {
    return expect(
      this.page.locator('text=No completed sessions with this exercise yet.')
    ).toBeVisible();
  }
}
