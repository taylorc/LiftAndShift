using System.Reflection;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Domain.Common;
using LiftAndShift.Infrastructure.Data;
using LiftAndShift.Web.Infrastructure;
using NetArchTest.Rules;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.ArchitectureTests;

public class DependencyTests
{
    private static readonly Assembly DomainAssembly = typeof(BaseEntity).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(IApplicationDbContext).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(ApplicationDbContext).Assembly;
    private static readonly Assembly WebAssembly = typeof(IEndpointGroup).Assembly;

    private const string DomainNamespace = "LiftAndShift.Domain";
    private const string ApplicationNamespace = "LiftAndShift.Application";
    private const string InfrastructureNamespace = "LiftAndShift.Infrastructure";
    private const string WebNamespace = "LiftAndShift.Web";

    [Test]
    public void Domain_Should_Not_HaveDependencyOnOtherProjects()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, WebNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Test]
    public void Application_Should_Not_HaveDependencyOnOtherProjects()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOnAny(InfrastructureNamespace, WebNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Test]
    public void Infrastructure_Should_Not_HaveDependencyOnWeb()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn(WebNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Test]
    public void Web_Should_Not_HaveDependencyOnInfrastructureInternals()
    {
        // Web is allowed to reference Infrastructure (for DI registration), but handlers/endpoints
        // should route through Application's abstractions rather than reaching into Infrastructure's
        // concrete data-access types directly.
        var result = Types.InAssembly(WebAssembly)
            .That()
            .ResideInNamespace("LiftAndShift.Web.Endpoints")
            .Should()
            .NotHaveDependencyOn("LiftAndShift.Infrastructure.Data")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }
}
