@Programme
Feature: Programme
    An authenticated user adopts a training programme

Scenario: User adopts the Starting Strength programme
    Given an authenticated user visits the programme page with no active programme
    When they start the "Starting Strength" programme
    Then their active programme is "Starting Strength"
    And their next session is visible
