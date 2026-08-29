import { expect } from '@playwright/test';
import { BasePage, baseUrl } from './base-page';

export class EditSessionPage extends BasePage {
  // Reached by clicking "Edit" on a past session row in ProgrammePage; never navigated to directly.
  get pagePath(): string {
    return `${baseUrl()}/programme`;
  }

  assertHeadingVisible() {
    return expect(this.page.locator('h1')).toHaveText('Edit session');
  }

  private setRows() {
    return this.page.locator('form article table tbody tr');
  }

  async incrementFirstWorkingSetCompletedReps() {
    const rows = this.setRows();
    // The heading renders immediately, but the sets only appear once the async
    // fetchProgrammeSessions() onMounted call resolves - wait for a row before counting.
    await rows.first().waitFor();
    const count = await rows.count();
    for (let i = 0; i < count; i++) {
      const row = rows.nth(i);
      const type = (await row.locator('td').nth(1).innerText()).trim();
      if (type !== 'Working') continue;

      const input = row.locator('td').nth(4).locator('input');
      const current = Number(await input.inputValue());
      await input.fill(String(current + 1));
      return;
    }
    throw new Error('No working set found to edit');
  }

  clickSaveChanges() {
    return this.page.locator("button:has-text('Save changes')").click();
  }
}
