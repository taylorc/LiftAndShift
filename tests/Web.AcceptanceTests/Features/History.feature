@History
Feature: Workout History
    An authenticated user reviews their past workout sessions

Scenario: User sees a logged workout in their history
    Given an authenticated user has logged a draft workout for "Bench Press"
    When they visit the history page
    Then the "Bench Press" session is visible with status "Draft"
    When they view the "Bench Press" session
    Then they are taken to its workout detail page
