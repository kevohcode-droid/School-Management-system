using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SchoolErp.Application.Common.Interfaces;

namespace SchoolErp.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef` build the context without booting the WebAPI host. Uses a
/// no-op current user; the connection string can be overridden via the
/// SCHOOLERP_CONNECTION environment variable.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SCHOOLERP_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=schoolerp;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ApplicationDbContext(options, new DesignTimeCurrentUser());
    }

    private sealed class DesignTimeCurrentUser : ICurrentUser
    {
        public bool IsAuthenticated => false;
        public string? UserId => null;
        public string? UserName => null;
        public Guid? TenantId => null;
        public IReadOnlyCollection<string> Roles => Array.Empty<string>();
    }
}
