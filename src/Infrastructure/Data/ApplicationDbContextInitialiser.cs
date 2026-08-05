using LiftAndShift.Domain.Constants;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Domain.ValueObjects;
using LiftAndShift.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiftAndShift.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            // See https://jasontaylor.dev/ef-core-database-initialisation-strategies
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var administratorRole = new IdentityRole(Roles.Administrator);

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }

        // Default users
        var administrator = new ApplicationUser { UserName = "administrator@localhost.com", Email = "administrator@localhost.com" };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new [] { administratorRole.Name });
            }
        }

        // Default data
        // Seed, if necessary
        if (!_context.TodoLists.Any())
        {
            _context.TodoLists.Add(new TodoList
            {
                Title = "Tasks",
                Colour = Colour.Green,
                Items =
                {
                    new TodoItem { Title = "Make a todo list 📃" },
                    new TodoItem { Title = "Check off the first item ✅" },
                    new TodoItem { Title = "Realise you've already done two things on the list! 🤯"},
                    new TodoItem { Title = "Reward yourself with a nice, long nap 🏆" },
                }
            });

            await _context.SaveChangesAsync();
        }

        // Seed exercises
        if (!_context.Exercises.Any())
        {
            var exercises = new List<Exercise>
            {
                // Starting Strength core lifts
                new() { Name = "Squat", Description = "Barbell back squat — primary lower body compound movement.", MuscleGroup = MuscleGroup.Legs, EquipmentType = EquipmentType.Barbell, MovementPattern = MovementPattern.Squat, IsCustom = false, IsActive = true },
                new() { Name = "Bench Press", Description = "Barbell flat bench press — primary horizontal push.", MuscleGroup = MuscleGroup.Chest, EquipmentType = EquipmentType.Barbell, MovementPattern = MovementPattern.Push, IsCustom = false, IsActive = true },
                new() { Name = "Deadlift", Description = "Conventional barbell deadlift — primary hip hinge.", MuscleGroup = MuscleGroup.Back, EquipmentType = EquipmentType.Barbell, MovementPattern = MovementPattern.Hinge, IsCustom = false, IsActive = true },
                new() { Name = "Overhead Press", Description = "Barbell overhead press — primary vertical push.", MuscleGroup = MuscleGroup.Shoulders, EquipmentType = EquipmentType.Barbell, MovementPattern = MovementPattern.Push, IsCustom = false, IsActive = true },
                new() { Name = "Power Clean", Description = "Barbell power clean from floor to rack position.", MuscleGroup = MuscleGroup.Full, EquipmentType = EquipmentType.Barbell, MovementPattern = MovementPattern.Hinge, IsCustom = false, IsActive = true },
                new() { Name = "Pendlay Row", Description = "Barbell row with plates touching floor each rep.", MuscleGroup = MuscleGroup.Back, EquipmentType = EquipmentType.Barbell, MovementPattern = MovementPattern.Pull, IsCustom = false, IsActive = true },
                // Additional common exercises
                new() { Name = "Romanian Deadlift", Description = "Hip hinge with slight knee bend — great for hamstrings.", MuscleGroup = MuscleGroup.Legs, EquipmentType = EquipmentType.Barbell, MovementPattern = MovementPattern.Hinge, IsCustom = false, IsActive = true },
                new() { Name = "Incline Bench Press", Description = "Barbell bench press on incline — upper chest emphasis.", MuscleGroup = MuscleGroup.Chest, EquipmentType = EquipmentType.Barbell, MovementPattern = MovementPattern.Push, IsCustom = false, IsActive = true },
                new() { Name = "Barbell Row", Description = "Bent-over barbell row — vertical back pull.", MuscleGroup = MuscleGroup.Back, EquipmentType = EquipmentType.Barbell, MovementPattern = MovementPattern.Pull, IsCustom = false, IsActive = true },
                new() { Name = "Pull-Up", Description = "Bodyweight vertical pull — lat and bicep dominant.", MuscleGroup = MuscleGroup.Back, EquipmentType = EquipmentType.Bodyweight, MovementPattern = MovementPattern.Pull, IsCustom = false, IsActive = true },
                new() { Name = "Dip", Description = "Bodyweight parallel bar dip — chest and tricep.", MuscleGroup = MuscleGroup.Chest, EquipmentType = EquipmentType.Bodyweight, MovementPattern = MovementPattern.Push, IsCustom = false, IsActive = true },
                new() { Name = "Dumbbell Curl", Description = "Dumbbell bicep curl.", MuscleGroup = MuscleGroup.Arms, EquipmentType = EquipmentType.Dumbbell, MovementPattern = MovementPattern.Pull, IsCustom = false, IsActive = true },
                new() { Name = "Tricep Pushdown", Description = "Cable tricep pushdown.", MuscleGroup = MuscleGroup.Arms, EquipmentType = EquipmentType.Cable, MovementPattern = MovementPattern.Push, IsCustom = false, IsActive = true },
                new() { Name = "Plank", Description = "Isometric core stability hold.", MuscleGroup = MuscleGroup.Core, EquipmentType = EquipmentType.Bodyweight, MovementPattern = MovementPattern.Carry, IsCustom = false, IsActive = true },
                new() { Name = "Leg Press", Description = "Machine leg press.", MuscleGroup = MuscleGroup.Legs, EquipmentType = EquipmentType.Machine, MovementPattern = MovementPattern.Squat, IsCustom = false, IsActive = true },
                new() { Name = "Lat Pulldown", Description = "Cable lat pulldown.", MuscleGroup = MuscleGroup.Back, EquipmentType = EquipmentType.Cable, MovementPattern = MovementPattern.Pull, IsCustom = false, IsActive = true },
            };

            _context.Exercises.AddRange(exercises);
            await _context.SaveChangesAsync();
        }
    }
}
