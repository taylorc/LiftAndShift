using System.Linq;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Events;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Domain.UnitTests.Entities;

public class TodoItemTests
{
    [Test]
    public void SettingDoneToTrueShouldRaiseTodoItemCompletedEvent()
    {
        var item = new TodoItem();

        item.Done = true;

        item.DomainEvents.Count.ShouldBe(1);
        var domainEvent = item.DomainEvents.Single().ShouldBeOfType<TodoItemCompletedEvent>();
        domainEvent.Item.ShouldBe(item);
    }

    [Test]
    public void SettingDoneToFalseShouldNotRaiseDomainEvent()
    {
        var item = new TodoItem();

        item.Done = false;

        item.DomainEvents.ShouldBeEmpty();
    }

    [Test]
    public void SettingDoneToTrueWhenAlreadyDoneShouldNotRaiseAnotherEvent()
    {
        var item = new TodoItem { Done = true };
        item.ClearDomainEvents();

        item.Done = true;

        item.DomainEvents.ShouldBeEmpty();
    }

    [Test]
    public void SettingDoneToFalseAfterCompletedShouldNotRaiseEvent()
    {
        var item = new TodoItem { Done = true };
        item.ClearDomainEvents();

        item.Done = false;

        item.DomainEvents.ShouldBeEmpty();
        item.Done.ShouldBeFalse();
    }

    [Test]
    public void DoneShouldReflectAssignedValue()
    {
        var item = new TodoItem();

        item.Done = true;

        item.Done.ShouldBeTrue();
    }
}
