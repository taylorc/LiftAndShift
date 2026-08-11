import { test } from '../fixtures';
import { CalculatorsPage } from '../pages/calculators.page';

test('user calculates plates for a target weight', async ({ authenticatedPage }) => {
  const calculatorsPage = new CalculatorsPage(authenticatedPage);
  await calculatorsPage.goto();

  await calculatorsPage.setPlateTargetWeight('100');
  await calculatorsPage.clickCalculate();

  await calculatorsPage.assertPlateResultVisible();
});

test('user calculates a warmup progression', async ({ authenticatedPage }) => {
  const calculatorsPage = new CalculatorsPage(authenticatedPage);
  await calculatorsPage.goto();

  await calculatorsPage.clickWarmupCalculatorTab();
  await calculatorsPage.setWarmupWorkingWeight('100');
  await calculatorsPage.clickCalculate();

  await calculatorsPage.assertWarmupTableVisible();
});
