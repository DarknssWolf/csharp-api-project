# CSharpAPIProject

A multi-tenant survey and voting API built with ASP.NET Core, following clean architecture and domain-driven design. Vendors run surveys and vote sets for their customers; a platform administrator manages the vendors. Written as a portfolio sample, so the scope is deliberately small but the plumbing is production-shaped.

## What it does

- **Surveys** — vendors create surveys with multiple choice, multi-select, and open text questions, including conditional child questions. Customers submit responses.
- **Voting** — vote sets with vote items that customers submit votes against.
- **Users and accounts** — registration, login, password reset, email change. Three roles: Administrator, Vendor, Customer.
- **Multi-tenancy** — every vendor is a tenant. The tenant is resolved from the `X-Tenant-Id` request header and enforced through EF Core global query filters, so a vendor only ever sees their own data. The platform administrator lives on tenant `0`.
- **Auth** — ASP.NET Identity with JWT bearer tokens (30 minute expiry plus refresh tokens) and policy-based authorization per role.

### Solution layout

| Project | Purpose |
|---|---|
| `CSharp-API-Project.Domain` | Entities, enums, roles and policy names. No dependencies. |
| `CSharp-API-Project.Application` | CQRS commands/queries (MediatR), validators (FluentValidation), pipeline behaviours for auth and validation. |
| `CSharp-API-Project.Infrastructure` | EF Core DbContext, migrations, Identity, tenant resolution, seeders. |
| `CSharp-API-Project.API` | Minimal API endpoints, JWT generation, OpenAPI/Scalar UI, NSwag client generation. |
| `CSharp-API-Project.AppHost` | .NET Aspire host that wires up the SQL Server container and the API. |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Docker Desktop (the AppHost spins up SQL Server in a container)
- EF Core CLI tools: `dotnet tool install -g dotnet-ef`

## Running the project

First run only — start the AppHost so Aspire creates the SQL Server container, then apply the schema:

```
dotnet run --project CSharp-API-Project.AppHost
```

The container (`sql`) is persistent and pinned to `127.0.0.1:1433` with the `sa` password from `AppHost.cs`, so you can also connect with Management Studio. Once it's up, stop the AppHost and run the migrations:

```
dotnet ef database update --project CSharp-API-Project.Infrastructure --startup-project CSharp-API-Project.API
```

Then start the AppHost again. The API seeds its roles, an administrator, a sample vendor with a default region, five customers, and sample survey and vote data on startup.

```
dotnet run --project CSharp-API-Project.AppHost
```

- API: http://localhost:5000 / https://localhost:5001
- Scalar UI: https://localhost:5001 (the root redirects to `/scalar/v1`)
- Swagger UI: https://localhost:5001/swagger

The API can also run on its own against the existing container: `dotnet run --project CSharp-API-Project.API --launch-profile http` (listens on http://localhost:5020).

Debug builds regenerate `wwwroot/swagger/specification.json` and the TypeScript client in `clients/` via NSwag. Skip that with `-p:SkipNSwag=true` if you're building without the app runnable.

## Sample sign in

Seeded accounts:

| Account | Password | Tenant | Role |
|---|---|---|---|
| `administrator@local.com` | `Administrator1!` | `0` | Administrator |
| `vendor@localhost.com` | `Vendor1!` | `1` | Vendor |
| `customer1@localhost.com` … `customer5@localhost.com` | `Customer1!` | `1` | Customer |

The administrator is the platform owner on tenant `0`: it registers and manages vendors and can reach into any tenant's users. The vendor account is the tenant admin for the seeded "Sample Vendor" — it manages that tenant's surveys, vote sets, customers, and users, and never sees anything outside tenant `1`. The five customer accounts belong to the sample vendor and are the ones that submit survey responses and cast votes; a seeded vote set (`VOTE001`, "Best Improvement Idea") with two approved vote items is ready to vote on. More customers can be created via `POST /api/Customers/Create`, which gives each one a Customer-role login in the vendor's tenant.

Log in with the tenant id in the `X-Tenant-Id` header:

```
curl -k -X POST https://localhost:5001/api/Users/login \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 1" \
  -d '{"email":"vendor@localhost.com","password":"Vendor1!"}'
```

The response contains an `accessToken` (valid 30 minutes) and a `refreshToken`. Send the access token as a bearer token, keeping the tenant header on every call:

```
curl -k "https://localhost:5001/api/Surveys/List?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer <accessToken>" \
  -H "X-Tenant-Id: 1"
```

In the Scalar or Swagger UI, paste the raw token into the bearer auth field — the `Bearer` prefix is added for you.

Administrators sign in with `X-Tenant-Id: 0`; vendor and customer accounts use their vendor's id. Expired access tokens are renewed via `POST /api/Users/refreshToken`.
