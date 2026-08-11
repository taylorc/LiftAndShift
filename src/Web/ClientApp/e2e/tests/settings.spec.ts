import { expect, test } from '../fixtures';
import { SettingsPage } from '../pages/settings.page';

test('user changes their preferred weight unit', async ({ authenticatedPage }) => {
  const settingsPage = new SettingsPage(authenticatedPage);
  await settingsPage.goto();

  await settingsPage.selectLbs();
  await settingsPage.clickSaveSettings();

  await settingsPage.assertSavedMessageVisible();

  await settingsPage.goto();
  expect(await settingsPage.isLbsChecked()).toBe(true);
});
