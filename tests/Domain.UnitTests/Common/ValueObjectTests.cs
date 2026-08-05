using System.Collections.Generic;
using LiftAndShift.Domain.Common;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Domain.UnitTests.Common;

public class ValueObjectTests
{
    private class Money : ValueObject
    {
        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public decimal Amount { get; }

        public string Currency { get; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    private class OtherValueObject : ValueObject
    {
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield break;
        }
    }

    [Test]
    public void EqualsShouldReturnTrueGivenSameComponents()
    {
        var money1 = new Money(10m, "GBP");
        var money2 = new Money(10m, "GBP");

        money1.Equals(money2).ShouldBeTrue();
    }

    [Test]
    public void EqualsShouldReturnFalseGivenDifferentComponents()
    {
        var money1 = new Money(10m, "GBP");
        var money2 = new Money(20m, "GBP");

        money1.Equals(money2).ShouldBeFalse();
    }

    [Test]
    public void EqualsShouldReturnFalseGivenNull()
    {
        var money = new Money(10m, "GBP");

        money.Equals(null).ShouldBeFalse();
    }

    [Test]
    public void EqualsShouldReturnFalseGivenDifferentType()
    {
        var money = new Money(10m, "GBP");
        var other = new OtherValueObject();

        money.Equals(other).ShouldBeFalse();
    }

    [Test]
    public void GetHashCodeShouldBeEqualGivenEqualComponents()
    {
        var money1 = new Money(10m, "GBP");
        var money2 = new Money(10m, "GBP");

        money1.GetHashCode().ShouldBe(money2.GetHashCode());
    }

    [Test]
    public void EqualityOperatorShouldReturnTrueGivenBothNull()
    {
        Money? money1 = null;
        Money? money2 = null;

        (money1! == money2!).ShouldBeTrue();
    }

    [Test]
    public void EqualityOperatorShouldReturnFalseGivenOneNull()
    {
        var money1 = new Money(10m, "GBP");
        Money? money2 = null;

        (money1 == money2!).ShouldBeFalse();
        (money2! == money1).ShouldBeFalse();
    }

    [Test]
    public void InequalityOperatorShouldReturnTrueGivenDifferentComponents()
    {
        var money1 = new Money(10m, "GBP");
        var money2 = new Money(20m, "GBP");

        (money1 != money2).ShouldBeTrue();
    }

    [Test]
    public void InequalityOperatorShouldReturnFalseGivenEqualComponents()
    {
        var money1 = new Money(10m, "GBP");
        var money2 = new Money(10m, "GBP");

        (money1 != money2).ShouldBeFalse();
    }
}
