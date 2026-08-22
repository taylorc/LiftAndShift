using LiftAndShift.Domain.Events;
using Microsoft.Extensions.Logging;

namespace LiftAndShift.Application.TodoItems.EventHandlers;

public class LogTodoItemCompleted : INotificationHandler<TodoItemCompletedEvent>
{
    private readonly ILogger<LogTodoItemCompleted> _logger;

    public LogTodoItemCompleted(ILogger<LogTodoItemCompleted> logger)
    {
        _logger = logger;
    }

    public ValueTask Handle(TodoItemCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("LiftAndShift Domain Event: {DomainEvent}", notification.GetType().Name);

        return ValueTask.CompletedTask;
    }
}
