@WorkoutDetail
Feature: Workout Detail
    An authenticated user manages an individual workout session

Scenario: User marks a draft workout as complete
    Given an authenticated user has a draft workout for "Deadlift" open on its detail page
    When they mark the workout as complete
    Then the workout status shows "Completed"

Scenario: User duplicates a workout as a new draft
    Given an authenticated user has a draft workout for "Overhead Press" open on its detail page
    When they duplicate the workout
    Then they are taken to a new draft workout detail page
