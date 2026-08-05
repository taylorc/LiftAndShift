using System.Reflection;
using LiftAndShift.Application.Calculators;
using LiftAndShift.Application.Common.Behaviours;
using LiftAndShift.Application.Common.Models;
using LiftAndShift.Application.TodoLists.Queries.GetTodos;
using LiftAndShift.Domain.Entities;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var config = TypeAdapterConfig.GlobalSettings;

        // Register calculator services
        builder.Services.AddSingleton<WarmupCalculatorService>();
        builder.Services.AddSingleton<PlateCalculatorService>();

        // Register all mappings
        config.NewConfig<TodoList, TodoListDto>();
        config.NewConfig<TodoItem, TodoItemDto>()
            .Map(dest => dest.Priority, src => (int)src.Priority);
        config.NewConfig<TodoList, LookupDto>();
        config.NewConfig<TodoItem, LookupDto>();

        builder.Services.AddSingleton(config);

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
        });
    }
}

