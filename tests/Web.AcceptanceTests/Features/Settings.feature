@Settings
Feature: Settings
    An authenticated user manages their personal preferences

Scenario: User changes their preferred weight unit
    Given an authenticated user visits the settings page
    When they select pounds as their weight unit
    And they save their settings
    Then a settings saved confirmation is displayed
    And their preferred unit remains pounds after reloading the page
