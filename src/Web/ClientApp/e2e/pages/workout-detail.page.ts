import { expect } from '@playwright/test';
import { BasePage, baseUrl } from './base-page';

export class WorkoutDetailPage extends BasePage {
  get pagePath(): string {
    return `${baseUrl()}/workout`;
  }

  gotoWorkout(id: number) {
    return this.page.goto(`${baseUrl()}/workout/${id}`);
  }

  assertOnWorkoutDetailPage() {
    return expect(this.page).toHaveURL(/\/workout\/\d+$/);
  }

  assertExerciseVisible(name: string) {
    return expect(this.page.locator('article header strong', { hasText: name })).toBeVisible();
  }

  assertStatus(status: string) {
    return expect(this.page.locator('hgroup p')).toContainText(status);
  }

  clickMarkComplete() {
    return this.page.locator("button:has-text('Mark Complete')").click();
  }

  clickDuplicate() {
    return this.page.locator("button:has-text('Duplicate as New Draft')").click();
  }

  assertMarkCompleteButtonHidden() {
    return expect(this.page.locator("button:has-text('Mark Complete')")).toHaveCount(0);
  }
}
