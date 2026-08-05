using LiftAndShift.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiftAndShift.Application.UnitTests.Common.TestHelpers;

public static class ApplicationDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
