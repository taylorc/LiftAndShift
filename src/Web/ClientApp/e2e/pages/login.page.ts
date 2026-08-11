import { expect } from '@playwright/test';
import { BasePage, baseUrl } from './base-page';

export class LoginPage extends BasePage {
  get pagePath(): string {
    return `${baseUrl()}/login`;
  }

  setEmail(email: string) {
    return this.page.fill('#email', email);
  }

  setPassword(password: string) {
    return this.page.fill('#password', password);
  }

  clickLogin() {
    return this.page.locator("button[type='submit']").click();
  }

  logoutButtonText() {
    return this.page.locator("a:has-text('Log out')").textContent();
  }

  assertErrorVisible() {
    return expect(this.page.locator('#login-error')).toBeVisible();
  }
}
