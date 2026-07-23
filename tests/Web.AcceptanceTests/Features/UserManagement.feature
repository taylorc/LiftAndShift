@UserManagement
Feature: User Management
    An authenticated user can update their profile

Scenario: User updates their starting weights
    Given an authenticated user is on the account page
    When they submit updated starting weights
    Then saving their profile redirects them to the home page
