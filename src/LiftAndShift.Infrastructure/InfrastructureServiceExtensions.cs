using LiftAndShift.Core.Interfaces;
using LiftAndShift.Core.Services;
using LiftAndShift.Infrastructure.Data;
using LiftAndShift.Infrastructure.Data.Queries;
using LiftAndShift.UseCases.Contributors.List;
using LiftAndShift.UseCases.FamilyMembers.List;
using LiftAndShift.UseCases.PersonalRecords.Get;
using LiftAndShift.UseCases.WorkoutSessions.List;

namespace LiftAndShift.Infrastructure;
public static class InfrastructureServiceExtensions
{
  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    ConfigurationManager config,
    ILogger logger)
  {
    string? connectionString = config.GetConnectionString("SqliteConnection");
    Guard.Against.Null(connectionString);

    services.AddScoped<EventDispatchInterceptor>();
    services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

    services.AddDbContext<AppDbContext>((provider, options) =>
    {
      var eventDispatchInterceptor = provider.GetRequiredService<EventDispatchInterceptor>();
      options.UseSqlite(connectionString);
      options.AddInterceptors(eventDispatchInterceptor);
    });

    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
           .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>))
           .AddScoped<IListContributorsQueryService, ListContributorsQueryService>()
           .AddScoped<IDeleteContributorService, DeleteContributorService>()
           .AddScoped<IListFamilyMembersQueryService, ListFamilyMembersQueryService>()
           .AddScoped<IListWorkoutSessionsQueryService, ListWorkoutSessionsQueryService>()
           .AddScoped<IPersonalRecordsQueryService, PersonalRecordsQueryService>();

    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
