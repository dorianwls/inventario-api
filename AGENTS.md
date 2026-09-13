# Inventario API

## Commands

- Restore and build as CI does: `dotnet restore Inventario.Api.sln && dotnet build Inventario.Api.sln --configuration Release --no-restore`.
- Run locally: `dotnet run --project Inventario.Api`.
- Create migrations with: `dotnet ef migrations add <Name> --project Inventario.Api --startup-project Inventario.Api --output-dir Persistence/Migrations`.

## Local database and configuration

- Start PostgreSQL 17 with `docker compose --env-file .env up -d`; copy `.env.example` to `.env` first and set `POSTGRES_PASSWORD`.
- Do not commit `.env` or `appsettings.Local.json`. Supply database credentials through `ConnectionStrings__Inventory`.
- Development startup applies committed migrations automatically and requires a reachable database. The design-time context has a password-less fallback, so set `ConnectionStrings__Inventory` when generating or applying migrations against a password-protected instance.
- JWT settings are mandatory. The development signing key is a placeholder and must not be used outside local development.
- An initial administrator is created only in Development when `BootstrapAdmin__Email` and `BootstrapAdmin__Password` are configured.

## Structure and behavior

- `Inventario.Api/Program.cs` is the composition root. Controllers require JWT by default; `/health` is anonymous. CORS permits only `http://localhost:5173`.
- `Auth/` owns ASP.NET Identity, GUID user IDs, roles, and token generation. Roles are `Administrator`, `Supervisor`, `Seller`, `Warehouse`, and `Accountant`.
- `Persistence/` owns `InventoryDbContext`, the design-time factory, and committed EF migrations. All database tables use the `inventory` PostgreSQL schema.
- Keep business entities in `Catalog/`, `Inventory/`, or `Commerce/`; controllers coordinate HTTP requests and persistence.
- Inventory changes must go through auditable `InventoryMovement` records. Preserve the transactional stock check: exits cannot produce negative stock.

## Verification

- CI builds only; it does not start Docker, apply migrations, or run tests. Run the Release build locally after changes.
