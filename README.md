# Jourlity

Jourlity is a journal system for professionals with a duty of confidentiality, such as lawyers and
medical professionals. It runs as an installed, offline-first desktop app. All client data is stored
on the user's own computer, and each case is encrypted separately. The backend handles only account
metadata, licence checks and subscriptions. It never sees client data.

## Core principles

These are hard requirements. Any change that conflicts with them is a bug.

1. **Client data stays on the device.** Journal entries, client details, documents and images are
   stored locally. They are never sent to the backend, to telemetry or to any third party.
2. **One database per case.** Each case gets its own database and its own encryption key. If one
   case is compromised, no other case or client is exposed.
3. **Up-to-date, well-tested encryption.** Use current, well-tested algorithms and libraries. Never
   invent crypto. Keys are never stored next to the data they protect.
4. **The backend stores metadata only.** It holds accounts, licence checks and subscription state,
   and nothing that identifies or describes a client.
5. **Offline-first.** Everything works without a network connection. The only exceptions are
   account checks, licence checks and subscription checks.

## Architecture

```
┌──────────────────────── User's computer ────────────────────────┐
│                                                                 │
│  Jourlity.App (.NET MAUI Blazor Hybrid)                         │
│     │                                                           │
│     ├── Jourlity.Data ──► local index DB  (client list)         │
│     │                └──► one DB per case (journal entries)     │
│     │                                                           │
│     └── Jourlity.Core ── BackendCommunicator ──────────┐        │
└────────────────────────────────────────────────────────┼────────┘
                                                         │ metadata only
                                                         ▼
                                   Jourlity.Api (ASP.NET Core)
                                   accounts · licensing · subscription
```

| Project                 | Purpose                                                                                  |
|-------------------------|------------------------------------------------------------------------------------------|
| `Jourlity.App`          | .NET MAUI Blazor Hybrid client: Razor UI, dependency injection setup, platform heads. Windows is the primary target. |
| `Jourlity.Core`         | Shared services and models used by the app, such as `BackendCommunicator` and the sign-in and sign-up models.        |
| `Jourlity.Data`         | EF Core + SQLite: entities, `JourlityContext` (local index), `CaseContext` (per-case database), the generic `Repository<TEntity, TContext>`, and migrations. |
| `Jourlity.DbMigrations` | Console startup project used by `dotnet ef` to generate migrations. It also seeds local test data.                  |
| `Jourlity.Api`          | ASP.NET Core Web API backend. It handles metadata only.                                                              |
| `Jourlity.Dto`          | Contracts shared between the app and the API, such as `HttpMessage<T>`.                                              |
| `Jourlity.Test`         | NUnit tests.                                                                                                         |

Local data lives under `%LOCALAPPDATA%\Jourlity\`. The local index database is at the root, and each
case database is in a subfolder of its own.

## Current state

The project is in early development. The principles above are not fully implemented yet:

- Local SQLite databases are **not yet encrypted**. This applies to both the local index and the
  case databases.
- The local index (`JourlityContext`) stores client names and emails. That is client data, so it
  must be encrypted as well.
- Error reports sent to the backend include the full exception. These reports have to be cleaned
  first so that no client data can leak through messages or stack traces.
- Authentication, licensing and subscription are not built yet.

## Getting started

### Prerequisites

- .NET 9 SDK
- The .NET MAUI workload: `dotnet workload install maui`
- Windows 10 version 1809 or later to run the Windows target
- Optional: the EF Core CLI, `dotnet tool install --global dotnet-ef`

### Build

```bash
dotnet build Jourlity.sln
```

To build only the Windows app:

```bash
dotnet build Jourlity.App/Jourlity.App.csproj -f net9.0-windows10.0.19041.0
```

### Run

1. Start the API. It listens on `http://localhost:5287`, and Swagger is available in Development:

   ```bash
   dotnet run --project Jourlity.Api --launch-profile http
   ```

2. Start the app with the **Windows Machine** launch profile, from your IDE or with:

   ```bash
   dotnet build Jourlity.App/Jourlity.App.csproj -f net9.0-windows10.0.19041.0 -t:Run
   ```

   The app reads the backend address from the `BackendUrl` environment variable. That variable is
   set in `Jourlity.App/Properties/launchSettings.json`. The app builds as an unpackaged Windows app,
   so the launch profile's environment variables apply.

### Tests

```bash
dotnet test Jourlity.Test
```

### Migrations

Run these from the solution folder. There is one migration set for each context:

```bash
dotnet ef migrations add <Name> --startup-project Jourlity.DbMigrations --project Jourlity.Data --context JourlityContext
```

```bash
dotnet ef migrations add <Name> --startup-project Jourlity.DbMigrations --project Jourlity.Data --context CaseContext
```

The app applies pending migrations automatically when it opens a database.

## Branching

The repository uses [git flow](https://nvie.com/posts/a-successful-git-branching-model/):

- `main`: production releases
- `develop`: integration branch, and the base for all work
- `feature/*`, `bugfix/*`: branch from `develop` and merge back through a pull request into `develop`
- `hotfix/*`: branch from `main`

Note: the remote also has a branch named `release`. Git cannot have a branch called `release`
alongside branches like `release/1.0`, so git flow release branches won't work until that branch
is renamed or the release prefix is changed.
