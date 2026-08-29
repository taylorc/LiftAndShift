import { BasePage, baseUrl } from './base-page';

export class LogSessionPage extends BasePage {
  get pagePath(): string {
    return `${baseUrl()}/log-session`;
  }

  private setRows() {
    return this.page.locator('form article table tbody tr');
  }

  async completeAllWorkingSets() {
    const rows = this.setRows();
    // The page navigates here via a client-side router.push, so the async onMounted fetch
    // (fetchActiveProgramme/fetchExercises) may still be in flight - wait for the form to
    // actually render before counting rows, or this silently no-ops on an empty locator.
    await rows.first().waitFor();
    const count = await rows.count();
    for (let i = 0; i < count; i++) {
      const row = rows.nth(i);
      const type = (await row.locator('td').nth(1).innerText()).trim();
      if (type !== 'Working') continue;

      const targetReps = await row.locator('td').nth(3).locator('input').inputValue();
      await row.locator('td').nth(4).locator('input').fill(targetReps);
      await row.locator('td').nth(5).locator('input[type="checkbox"]').check();
    }
  }

  clickCompleteSession() {
    return this.page.locator("button:has-text('Complete Session')").click();
  }

  async waitForWorkoutDetailUrlAndGetId(): Promise<number> {
    await this.page.waitForURL(/\/workout\/\d+$/);
    const match = /\/workout\/(\d+)$/.exec(this.page.url());
    return Number(match![1]);
  }
}
