using System.Linq;
using LiftAndShift.Domain.Exceptions;
using LiftAndShift.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Domain.UnitTests.ValueObjects;

public class ColourTests
{
    [Test]
    public void ShouldReturnCorrectColourCode()
    {
        var code = "#E05C4D";

        var colour = Colour.From(code);

        colour.Code.ShouldBe(code);
    }

    [Test]
    public void ToStringReturnsCode()
    {
        var colour = Colour.Red;

        colour.ToString().ShouldBe(colour.Code);
    }

    [Test]
    public void ShouldPerformImplicitConversionToColourCodeString()
    {
        string code = Colour.Red;

        code.ShouldBe("#E05C4D");
    }

    [Test]
    public void ShouldPerformExplicitConversionGivenSupportedColourCode()
    {
        var colour = (Colour)"#E05C4D";

        colour.ShouldBe(Colour.Red);
    }

    [Test]
    public void ShouldThrowUnsupportedColourExceptionGivenNotSupportedColourCode()
    {
        Should.Throw<UnsupportedColourException>(() => Colour.From("##FF33CC"));
    }

    [Test]
    public void ShouldBeComparableWithOperators()
    {
        var color1 = new Colour("#E05C4D");
        var color2 = new Colour("#E05C4D");
        var color3 = new Colour("#AAAAAA");
        (color1 == color2).ShouldBe(true);
        (color1 == color3).ShouldBe(false);
    }

    [Test]
    public void ShouldSupportNotEqualOperator()
    {
        var color1 = new Colour("#E05C4D");
        var color2 = new Colour("#AAAAAA");

        (color1 != color2).ShouldBe(true);
        (color1 != new Colour("#E05C4D")).ShouldBe(false);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ShouldDefaultToBlackGivenNullOrWhitespaceCode(string? code)
    {
        var colour = new Colour(code!);

        colour.Code.ShouldBe("#000000");
    }

    [Test]
    public void ShouldHaveEqualHashCodesForEqualColours()
    {
        var color1 = new Colour("#E05C4D");
        var color2 = new Colour("#E05C4D");

        color1.GetHashCode().ShouldBe(color2.GetHashCode());
    }

    [Test]
    public void EqualsShouldReturnFalseGivenNull()
    {
        var colour = Colour.Red;

        colour.Equals(null).ShouldBeFalse();
    }

    [Test]
    public void EqualsShouldReturnFalseGivenDifferentType()
    {
        var colour = Colour.Red;

        colour.Equals("#E05C4D").ShouldBeFalse();
    }

    [TestCase("#E05C4D")]
    [TestCase("#D98B2B")]
    [TestCase("#4CAF50")]
    [TestCase("#26A69A")]
    [TestCase("#5C6BC0")]
    [TestCase("#AB47BC")]
    [TestCase("#78909C")]
    public void FromShouldReturnColourGivenSupportedCode(string code)
    {
        var colour = Colour.From(code);

        colour.Code.ShouldBe(code);
    }

    [Test]
    public void SupportedColoursShouldContainAllNamedColours()
    {
        var supported = Colour.SupportedColours.ToList();

        supported.ShouldContain(Colour.Red);
        supported.ShouldContain(Colour.Orange);
        supported.ShouldContain(Colour.Green);
        supported.ShouldContain(Colour.Teal);
        supported.ShouldContain(Colour.Blue);
        supported.ShouldContain(Colour.Purple);
        supported.ShouldContain(Colour.Grey);
        supported.Count.ShouldBe(7);
    }

    [Test]
    public void UnsupportedColourExceptionShouldContainCodeInMessage()
    {
        const string code = "##FF33CC";

        var exception = Should.Throw<UnsupportedColourException>(() => Colour.From(code));

        exception.Message.ShouldBe($"Colour \"{code}\" is unsupported.");
    }
}
