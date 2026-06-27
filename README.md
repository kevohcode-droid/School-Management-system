# School ERP System

A multi-tenant School ERP built on **.NET 10** following **Clean Architecture**.

## Tech Stack
- .NET 10 Web API (C#)
- Clean / Onion Architecture (Domain → Application → Infrastructure → WebAPI)
- PostgreSQL + Entity Framework Core 10
- Multi-tenant: shared database with `TenantId` global query filters
- ASP.NET Core Identity + JWT + Role-Based Access Control (RBAC)

## Project Structure
```
src/
  frontend/
    SchoolErp.Frontend       Angular application
  backend/
    SchoolErp.Domain          Entities, enums, value objects, ITenantEntity
    SchoolErp.Application      Interfaces, DTOs, business logic (services), Result types
    SchoolErp.Infrastructure  EF Core DbContext + migrations, Identity, JWT, seeding
    SchoolErp.WebAPI          Controllers, JWT auth, RBAC policies, Swagger
  database/
    Migrations/               EF Core database migrations
    cleanup_sample_data.sql
    delete_demo_user.sql
```

## Multi-tenancy
Every tenant-scoped entity implements `ITenantEntity`. `ApplicationDbContext`
applies a global query filter `e.TenantId == CurrentTenantId` to each such type,
where `CurrentTenantId` comes from the authenticated user's JWT (`tenant_id`
claim). On insert, `SaveChanges` stamps `TenantId` (and audit fields) automatically.

## Roles (RBAC)
`SuperAdmin`, `Admin`, `Teacher`, `Student`, `Parent`. Controllers gate access
with `[Authorize(Roles = ...)]`. `SuperAdmin` manages tenants (schools).

## Running Locally
Prerequisites: .NET 10 SDK and a PostgreSQL instance.

1. Set the connection string in `src/backend/SchoolErp.WebAPI/appsettings.json`
   (`ConnectionStrings:DefaultConnection`) and the `Jwt` settings.
2. Run the API (migrations are applied and demo data is seeded on startup):
   ```bash
   dotnet run --project src/backend/SchoolErp.WebAPI
   ```
3. Open Swagger at `http://localhost:<port>/swagger`.

### Seeded demo credentials
| Field    | Value                |
|----------|----------------------|
| Tenant   | `demo`               |
| Email    | `admin@demo.school`  |
| Password | `Passw0rd!`          |
| Roles    | SuperAdmin, Admin    |

## Key Endpoints
- `POST /api/auth/register` — create a user in a tenant
- `POST /api/auth/login` — obtain a JWT
- `GET  /api/auth/me` — current user claims
- `GET/POST/PUT/DELETE /api/students` — student CRUD (Admin/Teacher)
- `GET/POST /api/tenants` — manage schools (SuperAdmin only)

## Migrations
```bash
dotnet ef migrations add <Name> \
  --project src/backend/SchoolErp.Infrastructure \
  --startup-project src/backend/SchoolErp.WebAPI \
  --output-dir Persistence/Migrations
```
