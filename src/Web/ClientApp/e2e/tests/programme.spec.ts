import { test } from '../fixtures';
import { ProgrammePage } from '../pages/programme.page';

test('user adopts the Starting Strength programme', async ({ authenticatedPage }) => {
  const programmePage = new ProgrammePage(authenticatedPage);
  await programmePage.goto();

  await programmePage.clickStartTemplate('Starting Strength');

  await programmePage.assertActiveProgrammeName('Starting Strength');
  await programmePage.assertNextSessionVisible();
});
