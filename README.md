# db-playground

A .NET 10 Web API for experimenting with Entity Framework Core migrations while switching between SQL Server and PostgreSQL in Docker Desktop.

When running in Development, the interactive Scalar API client is available at `http://localhost:5088/scalar` and the raw OpenAPI document is available at `http://localhost:5088/openapi/v1.json`.

## Prerequisites

- .NET SDK 10
- Docker Desktop running in Linux container mode
- `dotnet-ef` installed: `dotnet tool install --global dotnet-ef`

The `.env` file sets `PORT_PREFIX=62`. The database host ports are therefore `62143` for SQL Server and `62543` for PostgreSQL.

## Run with SQL Server

```powershell
docker compose up -d
dotnet ef database update --project src/DbPlayground.Api --context SqlServerMigrationDbContext
$env:Database__Provider = "SqlServer"
dotnet run --project src/DbPlayground.Api
```

## Run with PostgreSQL

Stop the other database first if it is running, then run:

```powershell
docker compose up -d
dotnet ef database update --project src/DbPlayground.Api --context PostgreSqlMigrationDbContext
$env:Database__Provider = "PostgreSql"
dotnet run --project src/DbPlayground.Api
```

## Create or update migrations

Migration files are provider-specific. Create a migration for each provider when the model changes:

```powershell
dotnet ef migrations add AddCustomerField --project src/DbPlayground.Api --context SqlServerMigrationDbContext --output-dir Migrations/SqlServer
dotnet ef migrations add AddCustomerField --project src/DbPlayground.Api --context PostgreSqlMigrationDbContext --output-dir Migrations/PostgreSql
```

Apply the matching set with `dotnet ef database update` and the corresponding provider argument.

## CRUD endpoints

- `GET /api/customers`
- `GET /api/customers/{id}`
- `POST /api/customers`
- `PUT /api/customers/{id}`
- `DELETE /api/customers/{id}`

Example request body:

```json
{
  "name": "Ada Lovelace",
  "email": "ada@example.com",
  "phone": "+1 555 0100",
  "birthDate": "1815-12-10"
}
```