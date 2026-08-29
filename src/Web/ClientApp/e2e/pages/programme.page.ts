import { expect } from '@playwright/test';
import { BasePage, baseUrl } from './base-page';

export class ProgrammePage extends BasePage {
  get pagePath(): string {
    return `${baseUrl()}/programme`;
  }

  assertHeading() {
    return expect(this.page.locator('h1')).toHaveText('Programme');
  }

  clickStartTemplate(templateName: string) {
    return this.page.locator(`button:has-text('Start ${templateName}')`).click();
  }

  assertActiveProgrammeName(name: string) {
    return expect(
      this.page.locator('article header strong', { hasText: name }).first()
    ).toBeVisible();
  }

  assertNextSessionVisible() {
    return expect(this.page.locator('h2', { hasText: 'Next Session' })).toBeVisible();
  }

  clickStartThisSession() {
    return this.page.locator("button:has-text('Start This Session')").click();
  }

  private pastSessionsSection() {
    return this.page.locator('section').filter({ has: this.page.getByRole('heading', { name: 'Past sessions', level: 2 }) });
  }

  pastSessionRows() {
    return this.pastSessionsSection().locator('tbody tr');
  }

  pastSessionsCount() {
    return this.pastSessionRows().count();
  }

  async clickEditOnRow(index: number) {
    await this.pastSessionRows().nth(index).locator("a:has-text('Edit')").click();
    // NuxtLink navigation is client-side; wait for the route to actually land before the
    // caller starts interacting with the edit page, rather than racing on heading text alone.
    await this.page.waitForURL(/\/programme\/session\/\d+\/edit$/);
  }

  clickDeleteOnRow(index: number) {
    return this.pastSessionRows().nth(index).locator("button:has-text('Delete')").click();
  }

  private metaForm() {
    return this.page.locator('form.programme-meta');
  }

  openEditDetails() {
    return this.page.locator("summary:has-text('Edit programme details')").click();
  }

  setStartDate(date: string) {
    return this.metaForm().locator("input[type='date']").fill(date);
  }

  selectStatus(label: 'Active' | 'Paused' | 'Abandoned') {
    return this.metaForm().locator('select').selectOption({ label });
  }

  async clickSaveDetails() {
    const button = this.metaForm().locator("button:has-text('Save details')");
    await button.click();
    // The save triggers an async store refresh (re-fetching the active programme), which
    // re-renders the form; wait for the busy state to clear before the caller interacts with
    // it again, or a fast follow-up action can hit a stale/detached element.
    await expect(button).toHaveAttribute('aria-busy', 'false', { timeout: 30_000 });
  }

  async assertSelectedStatusLabel(label: string) {
    const selected = await this.metaForm().locator('select option:checked').innerText();
    expect(selected.trim()).toBe(label);
  }

  assertStartDateValue(date: string) {
    return expect(this.metaForm().locator("input[type='date']")).toHaveValue(date);
  }
}
