import { test } from '../fixtures';
import { LogWorkoutPage } from '../pages/log-workout.page';
import { WorkoutDetailPage } from '../pages/workout-detail.page';

async function createDraftWorkout(logWorkoutPage: LogWorkoutPage, exerciseName: string) {
  await logWorkoutPage.goto();
  await logWorkoutPage.selectExerciseByName(exerciseName);
  await logWorkoutPage.clickAddExercise();
  await logWorkoutPage.setFirstSetWeight('60');
  await logWorkoutPage.clickSaveAsDraft();
  await logWorkoutPage.waitForWorkoutDetailUrlAndGetId();
}

test('user marks a draft workout as complete', async ({ authenticatedPage }) => {
  const logWorkoutPage = new LogWorkoutPage(authenticatedPage);
  const workoutDetailPage = new WorkoutDetailPage(authenticatedPage);

  await createDraftWorkout(logWorkoutPage, 'Deadlift');

  await workoutDetailPage.clickMarkComplete();

  await workoutDetailPage.assertStatus('Completed');
});

test('user duplicates a workout as a new draft', async ({ authenticatedPage }) => {
  const logWorkoutPage = new LogWorkoutPage(authenticatedPage);
  const workoutDetailPage = new WorkoutDetailPage(authenticatedPage);

  await createDraftWorkout(logWorkoutPage, 'Overhead Press');

  await workoutDetailPage.clickDuplicate();

  await workoutDetailPage.assertOnWorkoutDetailPage();
  await workoutDetailPage.assertStatus('Draft');
});
