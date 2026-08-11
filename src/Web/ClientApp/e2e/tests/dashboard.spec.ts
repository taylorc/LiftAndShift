import { test } from '../fixtures';
import { DashboardPage } from '../pages/dashboard.page';

test('user views their dashboard', async ({ authenticatedPage }) => {
  const dashboardPage = new DashboardPage(authenticatedPage);

  await dashboardPage.goto();

  await dashboardPage.assertHeading();
  await dashboardPage.assertPersonalRecordsSectionVisible();
  await dashboardPage.assertProgrammeCardVisible();
});
