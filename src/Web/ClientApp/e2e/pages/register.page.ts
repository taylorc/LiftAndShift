import { expect } from '@playwright/test';
import { BasePage, baseUrl } from './base-page';

export class RegisterPage extends BasePage {
  get pagePath(): string {
    return `${baseUrl()}/register`;
  }

  setEmail(email: string) {
    return this.page.fill('#email', email);
  }

  setPassword(password: string) {
    return this.page.fill('#password', password);
  }

  clickRegister() {
    return this.page.locator("button[type='submit']").click();
  }

  assertRedirectedToLogin() {
    return expect(this.page).toHaveURL(/.*\/login$/);
  }

  assertStillOnRegisterPage() {
    return expect(this.page).toHaveURL(/.*\/register$/);
  }

  assertPasswordHelperTextVisible() {
    return expect(this.page.locator('#password-helper')).toHaveText(
      'Password must be at least 6 characters.'
    );
  }
}
