import { expect } from '@playwright/test';
import { BasePage, baseUrl } from './base-page';

export class CalculatorsPage extends BasePage {
  get pagePath(): string {
    return `${baseUrl()}/calculators`;
  }

  assertHeading() {
    return expect(this.page.locator('h1')).toHaveText('Calculators');
  }

  clickWarmupCalculatorTab() {
    return this.page.locator("button:has-text('Warmup Calculator')").click();
  }

  setPlateTargetWeight(value: string) {
    return this.page.locator("section form input[type='number']").first().fill(value);
  }

  setWarmupWorkingWeight(value: string) {
    return this.page.locator("section form input[type='number']").first().fill(value);
  }

  clickCalculate() {
    return this.page.locator("button[type='submit']:has-text('Calculate')").click();
  }

  assertPlateResultVisible() {
    return expect(this.page.locator('text=Actual weight:')).toBeVisible();
  }

  assertWarmupTableVisible() {
    return expect(this.page.locator('table')).toBeVisible();
  }
}
