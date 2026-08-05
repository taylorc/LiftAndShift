@ExerciseLibrary
Feature: Exercise Library
    An authenticated user browses and manages exercises

Scenario: User searches for an exercise
    Given an authenticated user visits the exercise library page
    When they search for "Squat"
    Then the "Squat" exercise is visible in the list

Scenario: User adds and deletes a custom exercise
    Given an authenticated user visits the exercise library page
    When they add a custom exercise
    Then the custom exercise is visible in the list
    When they delete the custom exercise
    Then the custom exercise is no longer visible in the list
