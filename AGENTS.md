# AGENTS.md

## Project

ASP.NET Core 10 (net10.0) MVC + Web API app, single project at `src/backend/PaydayBackend.csproj`.
No `.sln` file, no test project, no CI yet.

## Environment setup

- Repo is managed with Nix `devenv` + `direnv` (`.envrc` runs `use devenv`). Enter the shell
  (`direnv allow` once, or `devenv shell`) to get `dotnet` 10 SDK and `dotnet-ef` on `PATH`.
- `devenv.nix` also provisions a local Postgres instance (db `payday`, user `kawid`) and exports its
  connection string as the `PAYDAY_DB` env var.
- `appsettings*.json` has **no** `ConnectionStrings` section on purpose — `Program.cs` falls back to
  `PAYDAY_DB` when `ConnectionStrings:ContractContext` is unset. Don't assume DB config is missing.

## Common commands

Run these from `src/backend` (there's no solution file to target from the repo root):

- Build: `dotnet build`
- Run: `dotnet run` (launch profiles in `Properties/launchSettings.json`: `http`, `https`,
  `http-staging`, `https-staging`)
- Staging matters: `Program.cs` only calls `UseStaticWebAssets()` when `Environment.IsStaging()`.

## EF Core migrations

Run from `src/backend` using `dotnet-ef` (already in the devenv shell):

- Add: `dotnet-ef migrations add <Name>`
- Apply: `dotnet-ef database update`
- The Postgres `citext` extension (used for `Payer.Email`) is enabled inside the `Init` migration
  itself via `AlterDatabase().Annotation("Npgsql:PostgresExtension:citext")` — don't add a manual
  `CREATE EXTENSION` statement anywhere.

## Architecture

- All entities inherit `Models/Abstractions/Entity.cs` (`int Id` PK) and map to Postgres schema
  `"Contract"` via `[Table("...", Schema = "Contract")]`.
- Generic repository pattern: `RepositoryBase<T>` (`Services/Repositories/Repository.cs`) implements
  CRUD; per-entity repos just subclass it (see `PayersRepository`).
- Generic cursor-based pagination: `EntityPaginatorService<TEntity, TRepo>`
  (`Services/Pagination/EntityPaginatorService.cs`). Cursors are opaque strings encrypted with
  ASP.NET `IDataProtector` (`CursorService`) — don't try to decode/construct them manually.
- Wiring lives in extension methods called from `Program.cs`: `AddRepositories()` and
  `AddPagination(...)`. To add a new entity: add its `DbSet` to `ContractContext`, create a
  migration, register it with `services.AddRepository<TEntity, IXRepository, XRepository>()`, and
  optionally `services.AddEntityPaginator<TEntity>()`.

## Gotchas / conventions

- VCS is Jujutsu (`jj`), colocated with git. Commit messages follow Conventional Commits with a
  path-like scope, e.g. `fix(backend/repository): fix Exist method`,
  `feat(backend/services): implement a simple repository pattern`. Match this style for new commits.
