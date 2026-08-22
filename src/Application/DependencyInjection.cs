using System.Reflection;
using LiftAndShift.Application.Calculators;
using LiftAndShift.Application.Common.Behaviours;
using LiftAndShift.Application.Common.Models;
using LiftAndShift.Application.TodoLists.Queries.GetTodos;
using LiftAndShift.Domain.Entities;
using Mapster;
using Mediator;
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

        builder.Services.AddMediator(options => {
            // Handlers depend on scoped services (IApplicationDbContext, IUser), so the mediator
            // and its pipeline behaviours must be scoped rather than the library's singleton default.
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [
                typeof(LoggingBehaviour<,>),
                typeof(UnhandledExceptionBehaviour<,>),
                typeof(AuthorizationBehaviour<,>),
                typeof(ValidationBehaviour<,>),
                typeof(PerformanceBehaviour<,>),
            ];
        });
    }
}

