using Microsoft.EntityFrameworkCore;

namespace DbPlayground.Api.Data;

public sealed class PostgreSqlMigrationDbContext(DbContextOptions<PostgreSqlMigrationDbContext> options)
    : CustomerDbContext(options);

public sealed class PostgreSqlMigrationDbContextFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<PostgreSqlMigrationDbContext>
{
    public PostgreSqlMigrationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PostgreSqlMigrationDbContext>()
            .UseNpgsql("Host=localhost;Port=62543;Database=dbplayground;Username=postgres;Password=postgres")
            .Options;

        return new PostgreSqlMigrationDbContext(options);
    }
}