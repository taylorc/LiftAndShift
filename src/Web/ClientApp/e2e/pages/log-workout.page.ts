import { expect } from '@playwright/test';
import { BasePage, baseUrl } from './base-page';

export class LogWorkoutPage extends BasePage {
  get pagePath(): string {
    return `${baseUrl()}/log-workout`;
  }

  assertHeading() {
    return expect(this.page.locator('h1')).toHaveText('Log Workout');
  }

  async selectExerciseByName(name: string) {
    const select = this.page.locator('select').first();
    const value = await select.locator('option', { hasText: name }).first().getAttribute('value');
    await select.selectOption(value);
  }

  clickAddExercise() {
    return this.page.locator("button:has-text('Add Exercise')").click();
  }

  setFirstSetWeight(value: string) {
    return this.page.locator("article table input[type='number']").first().fill(value);
  }

  markFirstSetCompleted() {
    return this.page.locator("article table input[type='checkbox']").first().check();
  }

  clickSaveAsDraft() {
    return this.page.locator("button.outline:has-text('Save as Draft')").click();
  }

  clickCompleteWorkout() {
    return this.page.locator("button:has-text('Complete Workout')").click();
  }

  async waitForWorkoutDetailUrlAndGetId(): Promise<number> {
    await this.page.waitForURL(/\/workout\/\d+$/);
    const match = /\/workout\/(\d+)$/.exec(this.page.url());
    return Number(match![1]);
  }
}
