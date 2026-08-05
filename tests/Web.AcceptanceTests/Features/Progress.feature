@Progress
Feature: Progress
    An authenticated user tracks their strength progress over time

Scenario: User views progress for an exercise with a completed session
    Given an authenticated user has completed a workout for "Pendlay Row" with a working set
    When they view their progress for "Pendlay Row"
    Then a progress data point is visible
