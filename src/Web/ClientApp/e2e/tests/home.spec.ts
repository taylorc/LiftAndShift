import { test } from '@playwright/test';
import { HomePage } from '../pages/home.page';

test('welcome heading is displayed', async ({ page }) => {
  const homePage = new HomePage(page);

  await homePage.goto();

  await homePage.assertHeading('Welcome');
});
