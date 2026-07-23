using LiftAndShift.Domain.Common;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Domain.UnitTests.Common;

public class BaseEntityTests
{
    private class TestEvent : BaseEvent
    {
    }

    private class TestEntity : BaseEntity
    {
    }

    [Test]
    public void DomainEventsShouldBeEmptyByDefault()
    {
        var entity = new TestEntity();

        entity.DomainEvents.ShouldBeEmpty();
    }

    [Test]
    public void AddDomainEventShouldAddEventToCollection()
    {
        var entity = new TestEntity();
        var domainEvent = new TestEvent();

        entity.AddDomainEvent(domainEvent);

        entity.DomainEvents.ShouldContain(domainEvent);
        entity.DomainEvents.Count.ShouldBe(1);
    }

    [Test]
    public void RemoveDomainEventShouldRemoveEventFromCollection()
    {
        var entity = new TestEntity();
        var domainEvent = new TestEvent();
        entity.AddDomainEvent(domainEvent);

        entity.RemoveDomainEvent(domainEvent);

        entity.DomainEvents.ShouldBeEmpty();
    }

    [Test]
    public void ClearDomainEventsShouldRemoveAllEvents()
    {
        var entity = new TestEntity();
        entity.AddDomainEvent(new TestEvent());
        entity.AddDomainEvent(new TestEvent());

        entity.ClearDomainEvents();

        entity.DomainEvents.ShouldBeEmpty();
    }
}
