using DbPlayground.Api.Data;
using DbPlayground.Api.Services;
using Microsoft.EntityFrameworkCore;
using Refit;
using Scalar.AspNetCore;
using System.Text.Json;
using System.Text.Json.Serialization;

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
builder.Services.AddDbContext<PostgreSqlMigrationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDbContext<SqlServerMigrationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services
    .AddRefitClient<IRulesApi>(new RefitSettings
    {
        ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        })
    })
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["RulesService:BaseUrl"] ?? "http://localhost:62600");
        var username = builder.Configuration["RulesService:Username"] ?? "kieserver";
        var password = builder.Configuration["RulesService:Password"] ?? "kieserver1!";
        var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    if (provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PostgreSqlMigrationDbContext>();
        dbContext.Database.Migrate();
    }
    else
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<SqlServerMigrationDbContext>();
        dbContext.Database.Migrate();
    }
}

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
