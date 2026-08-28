using System.Reflection;
using FluentValidation;
using LiftAndShift.Application.Common.Interfaces;
using Mediator;
using NetArchTest.Rules;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.ArchitectureTests;

public class NamingConventionTests
{
    private static readonly Assembly ApplicationAssembly = typeof(IApplicationDbContext).Assembly;

    // (handler type, name of the request type it handles), for every IRequestHandler<> / IRequestHandler<,> implementation.
    private static IEnumerable<(Type Handler, string RequestName)> RequestHandlers() =>
        from t in Types.InAssembly(ApplicationAssembly).GetTypes()
        from i in t.GetInterfaces()
        where i.IsGenericType &&
              (i.GetGenericTypeDefinition() == typeof(IRequestHandler<>) || i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
        select (t, i.GetGenericArguments()[0].Name);

    [Test]
    public void CommandHandlers_Should_HaveNameEndingWithCommandHandler()
    {
        var offenders = RequestHandlers()
            .Where(h => h.RequestName.EndsWith("Command", StringComparison.Ordinal))
            .Where(h => !h.Handler.Name.EndsWith("CommandHandler", StringComparison.Ordinal))
            .Select(h => h.Handler.FullName)
            .ToList();

        offenders.ShouldBeEmpty();
    }

    [Test]
    public void QueryHandlers_Should_HaveNameEndingWithQueryHandler()
    {
        var offenders = RequestHandlers()
            .Where(h => h.RequestName.EndsWith("Query", StringComparison.Ordinal))
            .Where(h => !h.Handler.Name.EndsWith("QueryHandler", StringComparison.Ordinal))
            .Select(h => h.Handler.FullName)
            .ToList();

        offenders.ShouldBeEmpty();
    }

    [Test]
    public void Validators_Should_HaveNameEndingWithValidator()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }
}
