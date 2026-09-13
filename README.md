# Inventario API

ASP.NET Core API for internal inventory control and accounting for a small Nicaraguan retailer.

## Technology

- .NET 10 and ASP.NET Core
- PostgreSQL (planned persistence)
- OpenAPI for development-time API documentation
- JWT authentication and role-based authorization (planned)

## Current foundation

- `GET /health` provides a public liveness endpoint.
- CORS permits the local Vite frontend at `http://localhost:5173`.
- The project targets `net10.0` and keeps OpenAPI dependencies patched.

## Run locally

```bash
dotnet restore
dotnet run --project Inventario.Api
```

OpenAPI is available in the Development environment at `/openapi/v1.json`.

## Planned modules

1. Identity and roles.
2. Product catalog and minimum stock levels.
3. Inventory movements and weighted-average costing.
4. Customer and supplier balances.
5. Double-entry accounting and opening balances.
6. Reports and low-stock email notifications.
