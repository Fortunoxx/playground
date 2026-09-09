# db-playground

[![Build](https://github.com/Fortunoxx/playground/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/Fortunoxx/playground/actions/workflows/build.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Docker Compose](https://img.shields.io/badge/Docker%20Compose-supported-2496ED?logo=docker&logoColor=white)](https://docs.docker.com/compose/)
[![Dependabot](https://img.shields.io/badge/Dependabot-enabled-025E8C?logo=dependabot&logoColor=white)](https://github.com/Fortunoxx/playground/network/updates)

A .NET 10 Web API for experimenting with Entity Framework Core migrations while switching between SQL Server and PostgreSQL in Docker Desktop. It also demonstrates Drools rules authored in KIE Business Central and evaluated through KIE Server by a typed Refit client.

The build workflow runs restore and Release builds for pushes and pull requests targeting `main`.

When running in Development, the interactive Scalar API client is available at `http://localhost:5088/scalar` and the raw OpenAPI document is available at `http://localhost:5088/openapi/v1.json`.

## Prerequisites

- .NET SDK 10
- Docker Desktop running in Linux container mode
- `dotnet-ef` installed: `dotnet tool install --global dotnet-ef`

The `.env` file sets `PORT_PREFIX=62`. The database host ports are therefore `62143` for SQL Server and `62543` for PostgreSQL. KIE Business Central is published on `62151` (with debug port `62150`), and KIE Server is published on `62600`. These host ports use the configured prefix and avoid Windows-reserved port ranges on this machine.

## Run with SQL Server

```powershell
docker compose up -d
dotnet ef database update --project src/DbPlayground.Api --context SqlServerMigrationDbContext
$env:Database__Provider = "SqlServer"
dotnet run --project src/DbPlayground.Api
```

`docker compose up -d` starts SQL Server, PostgreSQL, KIE Business Central, and KIE Server. The API calls KIE Server at `http://localhost:62600/kie-server/services/rest/server` through Refit. If the API is later containerized, configure `RulesService__BaseUrl` as `http://rules:8080/kie-server/services/rest/server` instead.

### KIE persistence

The Compose file uses named volumes for the KIE services:

- `business-central-git` stores Business Central's internal Git repositories, including spaces, projects, branches, and rule assets.
- `business-central-data` stores Business Central's WildFly runtime data.
- `kie-server-data` stores KIE Server's WildFly runtime data.

These volumes are reused when the KIE containers are recreated. Use `docker compose down` or `docker compose up -d --force-recreate` when restarting the services. Do not use `docker compose down -v` unless you intentionally want to delete the Business Central projects and deployed KIE state.

## Author and deploy the Drools rule

The rule is authored and deployed from KIE Business Central using the root Compose file.

1. Start the KIE services with `docker compose up -d rules business-central`. Starting the complete stack can also start the databases; if SQL Server host port `62143` is already in use, start only the KIE services or change `PORT_PREFIX`.
2. Open KIE Business Central at `http://localhost:62151/business-central` and sign in with `admin`.
3. Create or open the `MySpace` space.
4. Create the `orders-rules` project. Expand **Configure Advanced Options** and set these Maven coordinates explicitly:
   - Group ID: `com.myspace`
   - Artifact ID: `orders-rules`
   - Version: `1.0.0-SNAPSHOT`
     The explicit Artifact ID is important for the legacy Business Central image; leaving it implicit can produce a project with an invalid Maven POM.
5. On the project screen, choose **Import Asset**, select `rules/orderDecision.drl`, enter `orderDecision.drl` as the asset name, and keep package `com.myspace.orders_rules`.
6. Open the imported DRL. A Java `TestModel` data object is not required because the API inserts the order as a `Map` fact.
7. Keep the following contract in the DRL:
   - `global java.util.Map orderDecision;`
   - An inserted fact with `birthDate`, `evaluationAtUtc`, `productId`, `quantity`, and `rules`.
   - A result in `orderDecision` containing `allowed` and `reason`.
8. Validate the asset and confirm that Business Central reports `Item successfully validated` or a successful module build.
9. Save the asset.
10. Return to the project screen and select **Deploy**. A successful build alone is not enough; verify the KIE Server REST endpoint below.

### Add the deployment unit in Business Central

After the project has been built and deployed to the Business Central Maven repository, add it to the KIE Server configuration:

1. Open **Menu > Deploy > Execution Servers**.
2. Select the `docker-kie-server` server configuration.
3. Under **Deployment Units**, select **Add Deployment Unit**.
4. Enter or select the following Maven coordinates:
  - Name: `orders-rules`
  - Group Name: `com.myspace`
  - Artifact Id: `orders-rules`
  - Version: `1.0.0-SNAPSHOT`
  - Alias: leave empty
5. Leave **Start Deployment Unit?** checked, then select **Finish**.
6. Select the `docker-kie-server@rules:8080` remote server and confirm that the `orders-rules` container is listed and started.

The deployment-unit form only lists artifacts that Business Central can find in its Maven repository. If it shows **No artifacts available**, first build and deploy the `orders-rules` project from the project screen, then reopen **Add Deployment Unit**. The deployment unit cannot be created from the form until the artifact is published.

The Compose configuration must use the Docker-network address for controller callbacks and Maven resolution:

```yaml
KIE_SERVER_LOCATION: http://rules:8080/kie-server/services/rest/server
KIE_MAVEN_REPO: http://business-central:8080/business-central/maven2
```

The API still uses the host-published KIE Server address, `http://localhost:62600/kie-server/services/rest/server`.

If Business Central reports a successful deployment but the KIE Server container list is empty, create the container directly from the published project coordinates:

```powershell
$body = @'
{"container-id":"orders-rules","release-id":{"group-id":"com.myspace","artifact-id":"orders-rules","version":"1.0.0-SNAPSHOT"},"status":"STARTED"}
'@

$credential = Get-Credential
Invoke-WebRequest `
  -Uri "http://localhost:62600/kie-server/services/rest/server/containers/orders-rules" `
  -Method Put `
  -Credential $credential `
  -AllowUnencryptedAuthentication `
  -ContentType "application/json" `
  -Headers @{ Accept = "application/json" } `
  -Body $body
```

Use `kieserver` / `kieserver1!` for the credential prompt. A successful response has HTTP `201` and reports container status `STARTED`.

The deployed KIE session is `order-rules-session`, and the container ID used by the API is `order-rules`. The API sends the active product rules with their age limits and validity dates; the rule allows only positive quantities and customers who satisfy the deployed policy.

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

The database migration seeds product `1`, `Restricted Starter Product`, with a rule requiring the customer to be at least 18 years old. When an order is created, the main API sends a KIE Server command payload through `IRulesApi` and only persists the order when Drools returns an allowed result. Denied orders return `422 Unprocessable Entity`; an unavailable rules service returns `503 Service Unavailable`.

Example order request:

```json
{
  "customerId": 1,
  "productId": 1,
  "quantity": 2
}
```
