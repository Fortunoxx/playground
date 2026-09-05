using DbPlayground.Api.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var provider = builder.Configuration["Database:Provider"] ?? "SqlServer";
var connectionString = builder.Configuration.GetConnectionString(provider)
    ?? throw new InvalidOperationException($"Missing connection string for provider '{provider}'.");

builder.Services.AddDbContext<CustomerDbContext>(options =>
{
    if (provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(connectionString);
    }
    else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        throw new InvalidOperationException("Database:Provider must be SqlServer or PostgreSql.");
    }
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Db Playground API";
        options.Theme = ScalarTheme.Default;
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();
