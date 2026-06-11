using System.Runtime.CompilerServices;
using LiftAndShift.Application.Common.Models;
using LiftAndShift.Application.TodoLists.Queries.GetTodos;
using LiftAndShift.Domain.Entities;
using Mapster;
using NUnit.Framework;

namespace LiftAndShift.Application.UnitTests.Common.Mappings;

public class MappingTests
{
    private TypeAdapterConfig? _config;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _config = TypeAdapterConfig.GlobalSettings;

        // Register all mappings
        _config.NewConfig<TodoList, TodoListDto>();
        _config.NewConfig<TodoItem, TodoItemDto>()
            .Map(dest => dest.Priority, src => (int)src.Priority);
        _config.NewConfig<TodoList, LookupDto>();
        _config.NewConfig<TodoItem, LookupDto>();
    }

    [Test]
    [TestCase(typeof(TodoList), typeof(TodoListDto))]
    [TestCase(typeof(TodoItem), typeof(TodoItemDto))]
    public void ShouldSupportMappingFromSourceToDestination(Type source, Type destination)
    {
        var instance = GetInstanceOf(source);
        var mapper = new MapsterMapper.Mapper(_config!);

        // Verify mapping doesn't throw
        var result = mapper.Map(instance, source, destination);
        Assert.That(result, Is.Not.Null);
    }

    private static object GetInstanceOf(Type type)
    {
        if (type.GetConstructor(Type.EmptyTypes) != null)
            return Activator.CreateInstance(type)!;

        // Type without parameterless constructor
        return RuntimeHelpers.GetUninitializedObject(type);
    }
}
