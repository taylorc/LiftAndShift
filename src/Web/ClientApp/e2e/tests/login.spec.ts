import { expect, test } from '@playwright/test';
import { administratorEmail, administratorPassword } from '../auth';
import { LoginPage } from '../pages/login.page';

test('user can log in with valid credentials', async ({ page }) => {
  const loginPage = new LoginPage(page);
  await loginPage.goto();

  await loginPage.setEmail(administratorEmail);
  await loginPage.setPassword(administratorPassword);
  await loginPage.clickLogin();

  expect(await loginPage.logoutButtonText()).toBe('Log out');
});

test('user cannot log in with invalid credentials', async ({ page }) => {
  const loginPage = new LoginPage(page);
  await loginPage.goto();

  await loginPage.setEmail('hacker@localhost.com');
  await loginPage.setPassword('l337hax!');
  await loginPage.clickLogin();

  await loginPage.assertErrorVisible();
});
