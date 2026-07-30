# Nokpirab - Project Context

## Session Start

**Always read first:** `.claude/STATUS.md` (quick checkpoint, ~50 lines, local only — not in git)

---

## Overview

Nokpirab (นกพิราบ, Thai for "pigeon" 🕊️) is a lightweight, test-friendly **mediator** for the
CQRS pattern in .NET. It resolves a handler for a command or query out of the DI container and
invokes it — nothing more.

**This is a library, not an application.** It ships to strangers as the public NuGet package
[`Nokpirab`](https://www.nuget.org/packages/Nokpirab/) under MIT, from a **public** GitHub repo.
Every public type is a contract someone else's build depends on. That single fact drives most of
the rules below.

| | |
|---|---|
| Target framework | `net8.0` |
| SDK pin | `global.json` → `8.0.0`, `rollForward: latestMinor` (will **not** roll forward to 10) |
| Runtime dependency | `Microsoft.Extensions.DependencyInjection.Abstractions` 9.0.4 (only one) |
| Test stack | xUnit 2.5.3 · AutoFixture.AutoNSubstitute · NSubstitute · **Shouldly** · coverlet |
| CI | `.github/workflows/ci.yml` — restore/build/test in Release on push + PR to `main` |
| Packaging | `GeneratePackageOnBuild=true`, so a plain `dotnet build` already emits a `.nupkg` |

## Project Structure

```
src/
  Nokpirab/                       # the library (single project, single namespace `Nokpirab`)
    ICommand.cs                   # markers: ICommand, ICommand<TResult>
    IQuery.cs                     # marker:  IQuery<TResult>
    ICommandHandler.cs            # ICommandHandler<TCommand>, ICommandHandler<TCommand, TResult>
    IQueryHandler.cs              # IQueryHandler<TQuery, TResult>
    INokpirab.cs                  # the mediator abstraction (3 SendAsync overloads)
    Nokpirab.cs                   # internal implementation
    ServiceCollectionExtensions.cs# AddNokpirabFromAssembly
    HandlerNotFoundException.cs
tests/
  Nokpirab.Tests/
    NokpirabTests.cs
    ServiceCollectionExtensionsTests.cs
    FakeHandlers/                 # test doubles for the handler interfaces
.claude/                          # session context — local only, gitignored
.remember/                        # Remember plugin history — local only, self-gitignored
```

## Public API Surface

The whole contract, in one table. If a change is not in this table, it is not a breaking change.

| Type | Shape | Purpose |
|------|-------|---------|
| `ICommand` | marker interface | a command with no result |
| `ICommand<TResult>` | marker interface | a command that returns a result |
| `IQuery<TResult>` | marker interface | a query |
| `ICommandHandler<in TCommand>` | `Task HandleAsync(TCommand, CancellationToken = default)` | handles `ICommand` |
| `ICommandHandler<in TCommand, TResult>` | `Task<TResult> HandleAsync(...)` | handles `ICommand<TResult>` |
| `IQueryHandler<in TQuery, TResult>` | `Task<TResult> HandleAsync(...)` | handles `IQuery<TResult>` |
| `INokpirab` | 3 × `SendAsync` overloads | what consumers inject |
| `ServiceCollectionExtensions.AddNokpirabFromAssembly(this IServiceCollection, Assembly)` | extension method | registers `INokpirab` + scans and registers handlers |
| `HandlerNotFoundException` | `Exception` | thrown when the container has no handler for a request |

`Nokpirab` (the implementation of `INokpirab`) is deliberately **`internal`**, with
`[assembly: InternalsVisibleTo("Nokpirab.Tests")]` so tests can construct it directly.
It dispatches via `MakeGenericType` + `(dynamic)` and translates the container's
`InvalidOperationException` into `HandlerNotFoundException`.

## The public API is a contract (release gate)

A consumer upgrades the package without touching their code. Anything that makes their build or
their run fail is a **major** version. Before merging a change to `src/`, check it against this:

- **Additive only in a minor bump.** New types and new overloads are fine.
- **Adding a member to an existing public interface is breaking** — every consumer who implements
  `ICommandHandler<>` or `INokpirab` stops compiling. Major bump, or don't do it.
- **No renames, no signature changes, no removals** without a major bump.
- **Keep the implementation `internal`.** Anything made public can never be taken back.
- **DI lifetimes are observable behaviour.** Handlers and `INokpirab` are registered `Transient`
  today; changing that changes consumers' semantics, so treat it as breaking.
- **Widening the TFM is safe; narrowing is not.** Dropping `net8.0` cuts off LTS consumers.
- Bump `<Version>` in `src/Nokpirab/Nokpirab.csproj` in the same PR as the change it describes.

## Coding Standards

- **Tabs** for indentation in `src/` and `tests/` — the existing files use tabs, match them
- File-scoped namespaces, `Nullable` enabled, `ImplicitUsings` enabled
- Public interfaces, internal implementations
- Keep the dependency count at one — a mediator that drags in a graph defeats its own purpose
- Chained calls vertically aligned on the dots (see the global CLAUDE.md style rule)

## Test Conventions

- Naming: `Method_Condition_ShouldExpectedBehavior` — already the convention here
- `[Theory, AutoNSubstituteData]` when the test wants auto-generated substitutes; `[Fact]` otherwise
- **Shouldly** (`result.ShouldBe(expected)`) — not FluentAssertions, not `Assert.*`
- Arrange-Act-Assert, separated by blank lines with `// Arrange` / `// Act` / `// Assert` comments
- Test doubles live in `tests/Nokpirab.Tests/FakeHandlers/`
- Reflection-based registration is easy to break silently — cover
  `AddNokpirabFromAssembly` whenever the scanning logic changes

## Development Workflow

- **Main branch: `main`** (note: *not* `development` — this differs from IndyPOS/MimyShop)
- Feature branches off `main`, conventional commits, small focused PRs
- CI must be green before merge
- This is a personal repo → `gh auth switch --user purin-tavilsup` before using `gh` here

## Releasing

Publishing is automated. `.github/workflows/release.yml` fires on a `v*` tag and pushes to nuget.org
using [trusted publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing) (OIDC) —
**no API key is stored anywhere**, in the repo or in its settings.

1. Bump `<Version>` in `src/Nokpirab/Nokpirab.csproj`, checked against the contract rules above.
2. Merge to `main` and push.
3. `git tag vX.Y.Z && git push origin vX.Y.Z`

The workflow builds, tests **both** target frameworks, packs, then exchanges an OIDC token for a
one-hour key and pushes the `.nupkg` and `.snupkg`. It refuses to run when the tag does not match
`<Version>`, so a mistyped tag fails loudly instead of publishing the wrong version. Re-tagging
after a fix is safe — `--skip-duplicate` means a re-run will not error on a published version.

What will trip you up:

- **Run `gh auth switch --user purin-tavilsup` first.** The global git identity is a work account,
  and a per-repo `user.email` override does *not* cover the push credential — pushing as the wrong
  account fails with a bare `403`.
- **Renaming `release.yml` breaks publishing.** The nuget.org policy is bound to that exact file
  name (owner **Exconeer**, repo `purin-tavilsup/nokpirab`). Rename the file, update the policy.
- **`Your package was pushed.` does not mean available.** nuget.org validates (scan + signature)
  before indexing, which takes a few minutes. Confirm with
  `curl https://api.nuget.org/v3-flatcontainer/nokpirab/index.json`.
- **The GitHub release is manual:** `gh release create vX.Y.Z`. Automating it would need
  `contents: write` on the release job, which it deliberately does not have.
- **nuget.org shows no changelog** — `PackageReleaseNotes` is unset, so GitHub Releases is the only
  place a consumer can see what changed.

## Quick Commands

```bash
dotnet restore
dotnet build --configuration Release          # also produces the .nupkg
dotnet test                                   # xUnit
dotnet pack src/Nokpirab --configuration Release
```

## Not Applicable Here

The global CLAUDE.md carries Mimica-specific rules that have no meaning in this repo — there is
no logging abstraction (so no `LogCodes`), no database (so no `Mimica.SqlServer` module), and no
GCP. Ignore those sections while working here.
