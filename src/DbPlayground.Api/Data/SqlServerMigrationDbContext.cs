using Microsoft.EntityFrameworkCore;

namespace DbPlayground.Api.Data;

public sealed class SqlServerMigrationDbContext(DbContextOptions<SqlServerMigrationDbContext> options)
    : CustomerDbContext(options);

public sealed class SqlServerMigrationDbContextFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<SqlServerMigrationDbContext>
{
    public SqlServerMigrationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SqlServerMigrationDbContext>()
            .UseSqlServer("Server=localhost,62143;Database=DbPlayground;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True")
            .Options;

        return new SqlServerMigrationDbContext(options);
    }
}