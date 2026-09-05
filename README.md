# db-playground

[![Build](https://github.com/Fortunoxx/playground/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/Fortunoxx/playground/actions/workflows/build.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Docker Compose](https://img.shields.io/badge/Docker%20Compose-supported-2496ED?logo=docker&logoColor=white)](https://docs.docker.com/compose/)
[![Dependabot](https://img.shields.io/badge/Dependabot-enabled-025E8C?logo=dependabot&logoColor=white)](https://github.com/Fortunoxx/playground/network/updates)

A .NET 10 Web API for experimenting with Entity Framework Core migrations while switching between SQL Server and PostgreSQL in Docker Desktop. It also demonstrates a separate JBoss KIE Server/Drools container called through a typed Refit client.

The build workflow runs restore and Release builds for pushes and pull requests targeting `main`.

When running in Development, the interactive Scalar API client is available at `http://localhost:5088/scalar` and the raw OpenAPI document is available at `http://localhost:5088/openapi/v1.json`.

## Prerequisites

- .NET SDK 10
- Docker Desktop running in Linux container mode
- `dotnet-ef` installed: `dotnet tool install --global dotnet-ef`

The `.env` file sets `PORT_PREFIX=62`. The database host ports are therefore `62143` for SQL Server and `62543` for PostgreSQL. the JBoss KIE Server/Drools service is published on `62600`.

## Run with SQL Server

```powershell
docker compose up -d
dotnet ef database update --project src/DbPlayground.Api --context SqlServerMigrationDbContext
$env:Database__Provider = "SqlServer"
dotnet run --project src/DbPlayground.Api
```

`docker compose up -d` also starts the official `quay.io/kiegroup/kie-server-showcase:7.74.0.Final` image. The main API calls KIE Server at `http://localhost:62600/kie-server/services/rest/server` through Refit. If the main API is later containerized, configure `RulesService__BaseUrl` as `http://rules:8080/kie-server/services/rest/server` instead.

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

## Products, rules, and orders

Product endpoints:

- `GET /api/products`
- `GET /api/products/{id}`
- `POST /api/products`
- `PUT /api/products/{id}`
- `DELETE /api/products/{id}`
- `POST /api/products/{id}/rules`

Order endpoints:

- `GET /api/orders`
- `GET /api/orders/{id}`
- `POST /api/orders`
- `DELETE /api/orders/{id}`

The database migration seeds product `1`, `Restricted Starter Product`, with a rule requiring the customer to be at least 18 years old. When an order is created, the main API sends a KIE Server command payload through `IRulesApi` and only persists the order when Drools returns an allowed result. Denied orders return `422 Unprocessable Entity`; an unavailable rules service returns `503 Service Unavailable`. The KIE container named `order-rules` must contain the deployed Drools KJAR/rule session `order-rules-session` before order authorization can run.

Example order request:

```json
{
  "customerId": 1,
  "productId": 1,
  "quantity": 2
}
```