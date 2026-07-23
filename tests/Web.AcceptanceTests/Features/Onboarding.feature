@Onboarding
Feature: Onboarding
    An authenticated user sets up their starting lift weights

Scenario: User completes onboarding with valid starting weights
    Given an authenticated user is on the onboarding page
    When they submit valid starting weights
    Then they are redirected to the home page

Scenario: User cannot submit onboarding without a required weight
    Given an authenticated user is on the onboarding page
    When they submit the form without a squat starting weight
    Then they remain on the onboarding page
