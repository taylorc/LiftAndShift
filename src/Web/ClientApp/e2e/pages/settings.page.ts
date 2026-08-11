import { expect } from '@playwright/test';
import { BasePage, baseUrl } from './base-page';

export class SettingsPage extends BasePage {
  get pagePath(): string {
    return `${baseUrl()}/settings`;
  }

  assertHeading() {
    return expect(this.page.locator('h1')).toHaveText('Settings');
  }

  selectLbs() {
    return this.page.locator("input[type='radio'][value='lbs']").check();
  }

  selectKg() {
    return this.page.locator("input[type='radio'][value='kg']").check();
  }

  isLbsChecked() {
    return this.page.locator("input[type='radio'][value='lbs']").isChecked();
  }

  clickSaveSettings() {
    return this.page.locator("button:has-text('Save Settings')").click();
  }

  assertSavedMessageVisible() {
    return expect(this.page.locator('text=Settings saved.')).toBeVisible();
  }
}
