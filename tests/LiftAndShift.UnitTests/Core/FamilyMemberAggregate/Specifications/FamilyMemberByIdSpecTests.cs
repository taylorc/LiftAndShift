using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ardalis.Specification;
using AutoFixture;
using AutoFixture.AutoMoq;
using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.FamilyMemberAggregate.Specifications;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace LiftAndShift.UnitTests.Core.FamilyMemberAggregate.Specifications;

public class FamilyMemberByIdSpecTests
{
  [Fact]
  public void CreatesGivenValidValue()
  {
    var familyMemberId = FamilyMemberId.From(1);
    var spec = new FamilyMemberByIdSpec(familyMemberId);
    Assert.NotNull(spec);
  }

  [Fact]
  public void ThrowsGivenInvalidValue()
  {
    Assert.Throws<Vogen.ValueObjectValidationException>(() => new FamilyMemberByIdSpec(FamilyMemberId.From(0)));
  }

  [Fact]
  public void ThrowsGivenNegativeValue()
  {
    Assert.Throws<Vogen.ValueObjectValidationException>(() => new FamilyMemberByIdSpec(FamilyMemberId.From(-1)));
  }

  [Fact]
  public void GetsFamilyMemberById()
  {
    var fixture = new Fixture().Customize(new AutoMoqCustomization());

    var targetFamilyMemberId = FamilyMemberId.From(1);
    var targetFamilyMember = new FamilyMember(
        FamilyMemberName.From(fixture.Create<string>()),
        Pin.From("1234"))
    { Id = targetFamilyMemberId };

    var otherFamilyMember = new FamilyMember(
        FamilyMemberName.From(fixture.Create<string>()),
        Pin.From("5678"))
    { Id = FamilyMemberId.From(2) };

    var familyMembers = new List<FamilyMember> { targetFamilyMember, otherFamilyMember };

    var familyMembersDbSetMock = new Mock<DbSet<FamilyMember>>();
    MoqExtensions.ConfigureMock(familyMembersDbSetMock, familyMembers);

    var spec = new FamilyMemberByIdSpec(targetFamilyMemberId);

    var result = InMemorySpecificationEvaluator.Default
        .Evaluate(familyMembersDbSetMock.Object, spec)
        .SingleOrDefault();

    Assert.NotNull(result);
    Assert.Equal(targetFamilyMemberId, result.Id);
  }
}
