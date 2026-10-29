# CLAUDE.md

Guidance for Claude when working in this repository. See `README.md` for the architecture and the
build and run instructions.

## What this product is

Jourlity is a journal system for professionals with a duty of confidentiality, such as lawyers and
medical professionals. Users trust it with privileged and sensitive information about their clients.
Treat every change as if a breach would end someone's career: integrity and confidentiality come
before convenience, speed of delivery and new features.

## Non-negotiable rules

1. **Client data never leaves the device.** Client data means client names and contact details,
   journal entries, documents, images, case names, and anything derived from them. It must never be
   sent to `Jourlity.Api`, to logs that leave the machine, to telemetry, to crash reports or to any
   third-party service. If you are not sure whether something counts as client data, treat it as
   client data.
2. **The backend handles metadata only.** That means account information, authentication, licence
   checks and subscription state. Never add an endpoint, DTO or request that carries client data.
   DTOs in `Jourlity.Dto` must not contain client fields.
3. **One database and one key per case.** Each case has its own SQLite database (`CaseContext`) and
   its own encryption key. Never merge cases into a shared database, never share a key between
   cases, and never write a query that reads across case databases. The local index
   (`JourlityContext`) holds client data too, so it must be encrypted as well.
4. **Use up-to-date, well-tested cryptography only.**
   - Use well-maintained, widely used libraries and current algorithms with authenticated encryption.
     Never write crypto primitives or protocols by hand.
   - Never use deprecated algorithms or modes (for example MD5, SHA-1 for security, DES, ECB, or
     unauthenticated CBC).
   - Never hardcode keys, salts or IVs. Never reuse nonces. Never store a key next to the data it
     protects.
   - Never log keys, passphrases or decrypted content.
   - The concrete encryption design is **not decided yet**. Don't pick one on your own. Propose
     options and ask.
5. **Offline-first.** Every feature must work without a network connection. The only exceptions are
   sign-in, account checks, licence checks and subscription checks, and the app should fail
   gracefully when those can't reach the server. Licence problems are handled by the read-only
   mode described below, never by locking the user out of their data.
6. **Logging and errors must be clean.** Exception messages, stack traces and log lines can carry
   client data, such as entity values or file paths containing names. Anything sent off the device
   must be stripped of that first. Local logs must not contain client data either.
7. **Licence problems make the app read-only, never inaccessible.** The user's records belong to
   them, and they may be legally required to keep and produce them. The licence state decides what
   the app *allows*. It must never decide whether the data *can be opened*.

## Licence states and read-only mode

| State           | When                                                                        | Behaviour                                             |
|-----------------|-----------------------------------------------------------------------------|-------------------------------------------------------|
| **Active**      | The last successful validation is still within the grace period             | Full access                                           |
| **Grace**       | The server can't be reached, but the grace period hasn't run out            | Full access, with a non-blocking notice               |
| **Read-only**   | The grace period has run out without a successful validation, or the server says the licence is expired, cancelled or invalid | View, search, export and print only |

- **Being offline is not a licence failure.** Losing the network starts or continues the grace
  period. It doesn't switch the app to read-only straight away. The length of the grace period
  hasn't been decided yet.
- **Read-only means:** no creating, editing or deleting clients, cases or entries, and no schema
  migrations. Viewing, searching, exporting and printing keep working, so the user can always get
  their records out.
- **Export needs the correct encryption credentials.** It works in every licence state, read-only
  included, but only once the user has supplied the credentials that decrypt the case. A valid
  licence never replaces them, and an invalid licence never blocks them.
- **Enforce it in the app's service layer**, through one central licence-state service checked
  before every write. Hiding buttons alone isn't enough. Don't scatter licence checks through the
  UI.
- **Decryption must not depend on the licence or the server.** Keys for local data come only from
  the user's own credentials and local key storage, so read-only mode can always decrypt.
- **Store the last validation result locally so it can't be tampered with unnoticed**, and keep it
  metadata only: licence status, expiry and time of last check. A changed system clock must not
  extend the grace period indefinitely.
- **Read-only mode is a licensing measure, not a security boundary.** Confidentiality comes from
  encryption, not from the licence state.
- **Recover automatically.** As soon as validation succeeds again, the app returns to Active with no
  data loss and no reinstall.

## Platform

- The client is a **.NET MAUI Blazor Hybrid** app (`Jourlity.App`). It is an installed,
  offline-first desktop app, and Windows is the primary target.
- It is **not** a browser-hosted PWA. Don't move data into browser storage such as IndexedDB or
  localStorage, and don't add service-worker or web-hosting setups. Local data lives on the file
  system under `%LOCALAPPDATA%\Jourlity\`.

## When to stop and ask first

Ask before making any of these changes, even if the task seems to call for them:

- choosing or changing encryption, key derivation, or how keys are stored
- changing the licence states, the grace period, or what read-only mode allows
- changing the database layout, or how case databases are created, located or opened
- adding or changing anything sent to the backend, including new DTO fields or endpoints
- adding a third-party package, analytics, telemetry or crash reporting
- deleting or migrating existing user data, or any migration that drops columns or tables

If a request conflicts with the non-negotiable rules, say so and don't implement it.

## Codebase conventions

- **Solution layout:** `App` (UI and DI) → `Core` (services and models) → `Data` (EF Core) and `Dto`
  (shared contracts). `Api` depends only on `Dto`. Don't make `Api` reference `Data` or `Core`.
- **Data access:** go through `IRepository<TEntity>` / `Repository<TEntity, TContext>`. Register
  repositories in `MauiProgram.SettingUpServices` and contexts in `SettingUpDatabase`.
- **Entities:** inherit from `BaseEntity` (`Guid Id`, `CreatedAt`, `UpdatedAt`). Enum values are
  stored in the database, so never renumber existing values. Add new ones at the end with explicit
  numbers, like `EntryTypeEnum`.
- **Migrations:** keep one migration set for each context. Generate them with `Jourlity.DbMigrations`
  as the startup project; the commands are in the README. Never edit a migration that has already
  been committed. Add a new one instead.
- **HTTP:** register backend clients as typed `HttpClient`s with `AddHttpClient<TInterface, TImpl>`.
  The base address comes from the `BackendUrl` environment variable and must end in `/`. Request
  paths are relative and have no leading slash.
- **Blazor:** use `async Task` event handlers. Never block on async calls with `.Result` or
  `.GetAwaiter().GetResult()`.
- **Style:** .NET 9 with nullable reference types and implicit usings. Follow the
  surrounding code's naming and layout. The code uses Swedish-influenced names such as `Logg`; keep
  existing names consistent rather than "correcting" them.

## Build and test

```bash
dotnet build Jourlity.App/Jourlity.App.csproj -f net9.0-windows10.0.19041.0
```

```bash
dotnet build Jourlity.Api
```

```bash
dotnet test Jourlity.Test
```

Building the whole solution needs the MAUI workloads for every target platform. On Windows, build
the Windows target of the app directly. Before reporting work as done, build the projects you
changed, with no new warnings.

## Git workflow

- The repo uses git flow. Branch from `develop` as `feature/<name>` or `bugfix/<name>` and open PRs
  into `develop`. `main` holds releases.
- Never commit directly to `develop` or `main`, and never force-push shared branches.
- Never commit secrets, keys, real client data, or local database files (`*.sqlite`).
