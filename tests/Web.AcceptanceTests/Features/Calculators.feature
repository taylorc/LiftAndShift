@Calculators
Feature: Calculators
    An authenticated user uses the plate and warmup calculators

Scenario: User calculates plates for a target weight
    Given an authenticated user visits the calculators page
    When they calculate plates for 100 kg
    Then the plate calculator result is displayed

Scenario: User calculates a warmup progression
    Given an authenticated user visits the calculators page
    When they switch to the warmup calculator and calculate for 100 kg
    Then the warmup calculator result is displayed
