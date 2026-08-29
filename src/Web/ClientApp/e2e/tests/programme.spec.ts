import { expect, test } from '../fixtures';
import { ProgrammePage } from '../pages/programme.page';
import { LogSessionPage } from '../pages/log-session.page';
import { EditSessionPage } from '../pages/edit-session.page';

test('user adopts the Starting Strength programme', async ({ authenticatedPage }) => {
  const programmePage = new ProgrammePage(authenticatedPage);
  await programmePage.goto();

  await programmePage.clickStartTemplate('Starting Strength');

  await programmePage.assertActiveProgrammeName('Starting Strength');
  await programmePage.assertNextSessionVisible();
});

// The following tests build on the programme adopted above: logging a session, editing it, and
// deleting it all act on that same programme, and must run in this order within the file (the
// suite runs single-worker/non-parallel, so file-declaration order is guaranteed).

test('user logs their next programme session', async ({ authenticatedPage }) => {
  const programmePage = new ProgrammePage(authenticatedPage);
  const logSessionPage = new LogSessionPage(authenticatedPage);

  await programmePage.goto();
  await programmePage.clickStartThisSession();

  await logSessionPage.completeAllWorkingSets();
  await logSessionPage.clickCompleteSession();
  await logSessionPage.waitForWorkoutDetailUrlAndGetId();

  await programmePage.goto();
  expect(await programmePage.pastSessionsCount()).toBe(1);
  await programmePage.assertNextSessionVisible();
});

test('user edits a previously logged session', async ({ authenticatedPage }) => {
  const programmePage = new ProgrammePage(authenticatedPage);
  const editSessionPage = new EditSessionPage(authenticatedPage);

  await programmePage.goto();
  await programmePage.clickEditOnRow(0);

  await editSessionPage.assertHeadingVisible();
  await editSessionPage.incrementFirstWorkingSetCompletedReps();
  await editSessionPage.clickSaveChanges();

  await expect(authenticatedPage).toHaveURL(/\/programme$/);
});

test('user deletes their most recently logged session', async ({ authenticatedPage }) => {
  const programmePage = new ProgrammePage(authenticatedPage);

  await programmePage.goto();
  const countBefore = await programmePage.pastSessionsCount();

  await programmePage.clickDeleteOnRow(0);

  await expect(programmePage.pastSessionRows()).toHaveCount(countBefore - 1);
});

test('user edits their programme start date and status', async ({ authenticatedPage }) => {
  const programmePage = new ProgrammePage(authenticatedPage);

  await programmePage.goto();
  await programmePage.openEditDetails();
  await programmePage.setStartDate('2026-01-15');

  // GetActiveProgrammeQuery only returns Active-status programmes, so a reload after saving
  // Paused would show the "no active programme" screen instead of this form - exercise the
  // Paused write path, then switch back to Active in the same page load before reloading.
  await programmePage.selectStatus('Paused');
  await programmePage.clickSaveDetails();

  await programmePage.selectStatus('Active');
  await programmePage.clickSaveDetails();

  await programmePage.goto();
  await programmePage.openEditDetails();
  await programmePage.assertSelectedStatusLabel('Active');
  await programmePage.assertStartDateValue('2026-01-15');
});
