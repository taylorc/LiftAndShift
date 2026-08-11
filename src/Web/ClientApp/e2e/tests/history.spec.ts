import { test } from '../fixtures';
import { HistoryPage } from '../pages/history.page';
import { LogWorkoutPage } from '../pages/log-workout.page';
import { WorkoutDetailPage } from '../pages/workout-detail.page';

test('user sees a logged workout in their history', async ({ authenticatedPage }) => {
  const logWorkoutPage = new LogWorkoutPage(authenticatedPage);
  const historyPage = new HistoryPage(authenticatedPage);
  const workoutDetailPage = new WorkoutDetailPage(authenticatedPage);

  await logWorkoutPage.goto();
  await logWorkoutPage.selectExerciseByName('Bench Press');
  await logWorkoutPage.clickAddExercise();
  await logWorkoutPage.setFirstSetWeight('60');
  await logWorkoutPage.clickSaveAsDraft();
  await logWorkoutPage.waitForWorkoutDetailUrlAndGetId();

  await historyPage.goto();

  await historyPage.assertSessionVisible('Bench Press');
  await historyPage.assertStatusTag('Bench Press', 'Draft');

  await historyPage.clickViewFor('Bench Press');

  await workoutDetailPage.assertOnWorkoutDetailPage();
});
