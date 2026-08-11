import { expect, type Locator } from '@playwright/test';
import { BasePage, baseUrl } from './base-page';

export class ExerciseLibraryPage extends BasePage {
  get pagePath(): string {
    return `${baseUrl()}/exercise-library`;
  }

  assertHeading() {
    return expect(this.page.locator('h1')).toHaveText('Exercise Library');
  }

  search(term: string) {
    return this.page.fill("input[type='search']", term);
  }

  private exerciseNameCell(name: string): Locator {
    return this.page.locator('tbody tr td:first-child strong', { hasText: name });
  }

  private exerciseRow(name: string): Locator {
    return this.page
      .locator('tbody tr')
      .filter({ has: this.page.locator('td:first-child strong', { hasText: name }) });
  }

  assertExerciseRowVisible(name: string) {
    return expect(this.exerciseNameCell(name)).toBeVisible();
  }

  assertExerciseRowHidden(name: string) {
    return expect(this.exerciseNameCell(name)).toHaveCount(0);
  }

  openAddCustomExercise() {
    return this.page.locator("summary:has-text('Add Custom Exercise')").click();
  }

  setCustomExerciseName(name: string) {
    return this.page.getByLabel('Name *').fill(name);
  }

  clickAddExercise() {
    return this.page.locator("button:has-text('Add Exercise')").click();
  }

  clickDeleteFor(name: string) {
    return this.exerciseRow(name).locator("button:has-text('Delete')").click();
  }
}
