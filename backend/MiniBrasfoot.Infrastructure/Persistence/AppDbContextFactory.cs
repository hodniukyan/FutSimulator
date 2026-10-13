using BrasfootAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MiniBrasfoot.Infrastructure.Persistence;

// Inspect/generate scripts offline; this factory never applies migrations.
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "server=localhost;database=brasfootdb;user=root;password=;";
        var version = Environment.GetEnvironmentVariable("Database__ServerVersion") ?? "8.0.0";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(connection, ServerVersion.Parse(version)).Options;
        return new AppDbContext(options);
    }
}
