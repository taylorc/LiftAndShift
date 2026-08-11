import { randomUUID } from 'node:crypto';
import { test } from '../fixtures';
import { ExerciseLibraryPage } from '../pages/exercise-library.page';

test('user searches for an exercise', async ({ authenticatedPage }) => {
  const exerciseLibraryPage = new ExerciseLibraryPage(authenticatedPage);
  await exerciseLibraryPage.goto();

  await exerciseLibraryPage.search('Squat');

  await exerciseLibraryPage.assertExerciseRowVisible('Squat');
});

test('user adds and deletes a custom exercise', async ({ authenticatedPage }) => {
  authenticatedPage.on('dialog', (dialog) => dialog.accept());

  const exerciseLibraryPage = new ExerciseLibraryPage(authenticatedPage);
  await exerciseLibraryPage.goto();

  const name = `Custom Exercise ${randomUUID().replace(/-/g, '')}`;
  await exerciseLibraryPage.openAddCustomExercise();
  await exerciseLibraryPage.setCustomExerciseName(name);
  await exerciseLibraryPage.clickAddExercise();

  await exerciseLibraryPage.assertExerciseRowVisible(name);

  await exerciseLibraryPage.clickDeleteFor(name);

  await exerciseLibraryPage.assertExerciseRowHidden(name);
});
