import { expect, type BrowserContext, type Page } from '@playwright/test';
import { LoginPage } from './pages/login.page';

export const administratorEmail = 'administrator@localhost.com';
export const administratorPassword = 'Administrator1!';

export async function loginAsAdministrator(context: BrowserContext): Promise<Page> {
  const page = await context.newPage();
  const loginPage = new LoginPage(page);

  await loginPage.goto();
  await loginPage.setEmail(administratorEmail);
  await loginPage.setPassword(administratorPassword);
  await loginPage.clickLogin();

  await expect(page.locator("a:has-text('Log out')")).toBeVisible();

  return page;
}
