using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DbPlayground.Api.Data;

public sealed class CustomerDbContextFactory : IDesignTimeDbContextFactory<CustomerDbContext>
{
    public CustomerDbContext CreateDbContext(string[] args)
    {
        var provider = GetProvider(args);
        var connectionString = provider == "PostgreSql"
            ? "Host=localhost;Port=62543;Database=dbplayground;Username=postgres;Password=postgres"
            : "Server=localhost,62143;Database=DbPlayground;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<CustomerDbContext>();
        if (provider == "PostgreSql")
        {
            optionsBuilder.UseNpgsql(connectionString);
        }
        else
        {
            optionsBuilder.UseSqlServer(connectionString);
        }

        return new CustomerDbContext(optionsBuilder.Options);
    }

    private static string GetProvider(IEnumerable<string> args)
    {
        var providerArgument = args.FirstOrDefault(argument => argument.StartsWith("--provider=", StringComparison.OrdinalIgnoreCase));
        var provider = providerArgument?[(providerArgument.IndexOf('=') + 1)..];

        return provider?.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase) == true
            ? "PostgreSql"
            : "SqlServer";
    }
}