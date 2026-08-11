import { test } from '../fixtures';
import { LogWorkoutPage } from '../pages/log-workout.page';
import { ProgressPage } from '../pages/progress.page';

test('user views progress for an exercise with a completed session', async ({
  authenticatedPage,
}) => {
  const logWorkoutPage = new LogWorkoutPage(authenticatedPage);
  const progressPage = new ProgressPage(authenticatedPage);

  await logWorkoutPage.goto();
  await logWorkoutPage.selectExerciseByName('Pendlay Row');
  await logWorkoutPage.clickAddExercise();
  await logWorkoutPage.setFirstSetWeight('60');
  await logWorkoutPage.markFirstSetCompleted();
  await logWorkoutPage.clickCompleteWorkout();
  await logWorkoutPage.waitForWorkoutDetailUrlAndGetId();

  await progressPage.goto();
  await progressPage.selectExerciseByName('Pendlay Row');

  await progressPage.assertTableRowVisible();
});
