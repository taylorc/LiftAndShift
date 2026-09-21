using LiftAndShift.Core.ContributorAggregate;
using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.ProgrammeAggregate;
using LiftAndShift.Core.WorkWeightAggregate;

namespace LiftAndShift.Infrastructure.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Contributor> Contributors => Set<Contributor>();
  public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
  public DbSet<Programme> Programmes => Set<Programme>();
  public DbSet<WorkWeight> WorkWeights => Set<WorkWeight>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }

  public override int SaveChanges() =>
        SaveChangesAsync().GetAwaiter().GetResult();
}
