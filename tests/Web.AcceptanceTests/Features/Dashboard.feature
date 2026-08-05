@Dashboard
Feature: Dashboard
    An authenticated user sees their training overview

Scenario: User views their dashboard
    Given an authenticated user visits the dashboard page
    Then the dashboard heading is visible
    And the personal records section is visible
    And a link to the programme page is visible
