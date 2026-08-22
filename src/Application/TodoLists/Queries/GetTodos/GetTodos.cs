using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Models;
using LiftAndShift.Application.Common.Security;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Domain.ValueObjects;

namespace LiftAndShift.Application.TodoLists.Queries.GetTodos;

[Authorize]
public record GetTodosQuery : IRequest<TodosVm>;

public class GetTodosQueryHandler : IRequestHandler<GetTodosQuery, TodosVm>
{
    private readonly IApplicationDbContext _context;

    public GetTodosQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<TodosVm> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        return new TodosVm
        {
            PriorityLevels = Enum.GetValues(typeof(PriorityLevel))
                .Cast<PriorityLevel>()
                .Select(p => new LookupDto { Id = (int)p, Title = p.ToString() })
                .ToList(),

            Colours =
            [
                new ColourDto { Code = Colour.Grey, Name = nameof(Colour.Grey) },
                new ColourDto { Code = Colour.Purple, Name = nameof(Colour.Purple) },
                new ColourDto { Code = Colour.Blue, Name = nameof(Colour.Blue) },
                new ColourDto { Code = Colour.Teal, Name = nameof(Colour.Teal) },
                new ColourDto { Code = Colour.Green, Name = nameof(Colour.Green) },
                new ColourDto { Code = Colour.Orange, Name = nameof(Colour.Orange) },
                new ColourDto { Code = Colour.Red, Name = nameof(Colour.Red) },
            ],

            Lists = await _context.TodoLists
                .AsNoTracking()
                .ProjectToType<TodoListDto>()
                .OrderBy(t => t.Title)
                .ToListAsync(cancellationToken)
        };
    }
}
