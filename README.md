# School ERP System

A multi-tenant School ERP built on **.NET 10** following **Clean Architecture**.

## Tech Stack

- .NET 10 Web API (C#)
- Clean / Onion Architecture (Domain → Application → Infrastructure → WebAPI)
- PostgreSQL + Entity Framework Core 10
- Multi-tenant: shared database with `TenantId` global query filters
- ASP.NET Core Identity + JWT + Role-Based Access Control (RBAC)

## Project Structure

```text
src/
  SchoolErp.Frontend/         Angular application
  SchoolErp.Domain/           Entities, enums, value objects, ITenantEntity
  SchoolErp.Application/      Interfaces, DTOs, business logic and services
  SchoolErp.Infrastructure/   EF Core DbContext, migrations, Identity, JWT, seeding
  SchoolErp.WebAPI/           Controllers, JWT auth, authorization and Swagger
  database/                   SQL maintenance scripts
```

## Multi-tenancy

Every tenant-scoped entity implements `ITenantEntity`. `ApplicationDbContext`
applies a global query filter `e.TenantId == CurrentTenantId` to each such type,
where `CurrentTenantId` comes from the authenticated user's JWT (`tenant_id`
claim). On insert, `SaveChanges` stamps `TenantId` (and audit fields) automatically.

## Roles (RBAC)

The application roles are `SuperAdmin`, `Admin`, `Teacher`, `Accountant`,
`Staff`, `Student`, and `Parent`. API controllers enforce access with
`[Authorize(Roles = ...)]`; selecting a role on the login page does not grant
that role. The authenticated roles returned by the API must match the selected
login role. `SuperAdmin` manages tenants (schools).

- **Admin / SuperAdmin:** school-wide administration. Only administrators can
  review or finalize academic results and manage staff records.
- **Teacher:** teaching and academic functions allowed by the relevant API.
- **Accountant:** finance APIs and finance dashboard totals. Accountant access
  does not grant access to academic-management or administration APIs.
- **Staff:** dashboard access only by default. Staff are not automatically
  granted student, academic, or finance access. Per-employee permissions are
  not yet configurable in this application.
- **Student / Parent:** use the protected student and linked-child portal APIs
  for their own academic and related records.

Public registration is limited to Student and Parent roles. Administrators can
create Teacher, Staff, or Accountant login accounts while onboarding an employee
from Staff Management. The generated temporary password is shown once, stored
only as an Identity password hash, and must be changed at first sign-in.

## Running Locally

Prerequisites: .NET 10 SDK and a PostgreSQL instance.

1. Set `ConnectionStrings:DefaultConnection` and the `Jwt` settings in
   `src/SchoolErp.WebAPI/appsettings.json` (or use a local configuration
   override; do not commit production credentials).
2. Run the API (migrations are applied and local seed data is loaded on startup):

   ```bash
   dotnet run --project src/SchoolErp.WebAPI/SchoolErp.WebAPI.csproj
   ```

3. Open Swagger at `http://localhost:<port>/swagger`.
4. Run the Angular frontend:

   ```bash
   npm --prefix src/SchoolErp.Frontend install
   npm --prefix src/SchoolErp.Frontend start
   ```

### Local seed credentials

The following seeded credentials are for local development only. Demo role
accounts use the shared password `Test1234!`; the seeded administrator has the
separate password shown below. Replace or disable these accounts and credentials
before deploying to a non-development environment.

| Field    | Value                                             |
|----------|---------------------------------------------------|
| Tenant   | `100`                                             |
| Email    | `kevohkevi110@gmail.com`                          |
| Password | `Kevoh2060,!`                                     |
| Roles    | Admin                                             |

The seeder also creates demo accounts for `SuperAdmin`, `Teacher`, `Accountant`,
`Staff`, `Student`, and `Parent` using `superadmin@demo.com`,
`teacher@demo.com`, `accountant@demo.com`, `staff@demo.com`,
`student@demo.com`, and `parent@demo.com`, respectively. All use the shared
demo password above.

## Key Endpoints

- `POST /api/auth/register` — create a user in a tenant
- `POST /api/auth/login` — obtain a JWT
- `GET  /api/auth/me` — current user claims
- `POST /api/auth/change-password` — change the authenticated user's password
- `GET/POST/PUT/DELETE /api/students` — student CRUD (Admin/Teacher)
- `/api/fees/*` — finance operations (Admin/Accountant/SuperAdmin)
- `/api/performance/*` — academic marks and administrator result review
- `GET/POST /api/tenants` — manage schools (SuperAdmin only)

## Migrations

```bash
dotnet ef migrations add <Name> \
  --project src/SchoolErp.Infrastructure/SchoolErp.Infrastructure.csproj \
  --startup-project src/SchoolErp.WebAPI/SchoolErp.WebAPI.csproj \
  --output-dir Persistence/Migrations
```
