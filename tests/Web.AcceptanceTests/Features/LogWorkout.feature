@LogWorkout
Feature: Log Workout
    An authenticated user logs a workout session

Scenario: User logs a workout as a draft
    Given an authenticated user visits the log workout page
    When they add the "Squat" exercise with a working weight
    And they save the workout as a draft
    Then they are taken to the workout detail page showing "Squat" as a draft
