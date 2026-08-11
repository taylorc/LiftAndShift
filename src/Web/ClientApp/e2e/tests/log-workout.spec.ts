import { test } from '../fixtures';
import { LogWorkoutPage } from '../pages/log-workout.page';
import { WorkoutDetailPage } from '../pages/workout-detail.page';

test('user logs a workout as a draft', async ({ authenticatedPage }) => {
  const logWorkoutPage = new LogWorkoutPage(authenticatedPage);
  const workoutDetailPage = new WorkoutDetailPage(authenticatedPage);

  await logWorkoutPage.goto();

  await logWorkoutPage.selectExerciseByName('Squat');
  await logWorkoutPage.clickAddExercise();
  await logWorkoutPage.setFirstSetWeight('60');
  await logWorkoutPage.clickSaveAsDraft();

  await logWorkoutPage.waitForWorkoutDetailUrlAndGetId();
  await workoutDetailPage.assertExerciseVisible('Squat');
  await workoutDetailPage.assertStatus('Draft');
});
