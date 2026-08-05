using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Domain.UnitTests.Entities;

public class TodoListTests
{
    [Test]
    public void ShouldDefaultToGreyColour()
    {
        var list = new TodoList();

        list.Colour.ShouldBe(Colour.Grey);
    }

    [Test]
    public void ItemsShouldBeEmptyByDefault()
    {
        var list = new TodoList();

        list.Items.ShouldBeEmpty();
    }

    [Test]
    public void ItemsShouldContainAddedItem()
    {
        var list = new TodoList();
        var item = new TodoItem();

        list.Items.Add(item);

        list.Items.ShouldContain(item);
    }
}
