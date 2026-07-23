@Register
Feature: Register
    User can create a new account

Scenario: User can register with a valid email and password
    Given a visitor is on the register page
    When they register with a valid email and password
    Then they are redirected to the login page

Scenario: User cannot register with a password that is too short
    Given a visitor is on the register page
    When they submit the registration form with a password that is too short
    Then a password validation message is displayed
