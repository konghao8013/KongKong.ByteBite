---
name: bytebite-feature-locator
description: Locate and maintain KongKong.ByteBite feature implementation points across backend controllers/services, frontend API modules/pages/stores, tests, and project documentation. Use when Codex works on ByteBite features, bug fixes, backend API changes, frontend UI changes, tests, or any request that asks where a feature is implemented or requires feature documentation updates.
---

# ByteBite Feature Locator

Use this skill to find the exact backend and frontend implementation points for a ByteBite feature, then keep the project documentation aligned with the code change.

## Required Workflow

1. Read `docs/feature-map/README.md` first. Use it as the feature-to-code index.
2. Read the related requirement or status row in `docs/requirements/feature-implementation-checklist.md`.
3. Read the related domain spec and API contract from `docs/specs/` and `docs/contracts/` when behavior or endpoints may change.
4. Locate backend implementation through controllers, services, entities, hubs, seeders, migrations, and tests.
5. Locate frontend implementation through API modules, routes, pages, layouts, stores, composables, types, and components.
6. After implementing or fixing a feature, update `docs/feature-map/README.md` in the same turn whenever a method, endpoint, page, store, composable, model, test, or documentation link changes.
7. If the feature status changes, also use `docs/skills/bytebite-feature-checklist/SKILL.md` and update `docs/requirements/feature-implementation-checklist.md`.

## Search Recipes

Use focused searches before editing.

Backend:

```powershell
rg -n "FeatureName|Route|Http|Controller" src/ByteBite.Api src/ByteBite.Application
rg -n "public .*Async|class .*Service|record .*Input|class .*Input" src/ByteBite.Application/Services
rg -n "EntityName|DbSet|HasIndex|Migration" src/ByteBite.Infrastructure
```

Frontend:

```powershell
rg -n "apiMethod|request\.|path:|name:" web/src/api web/src/router web/src/pages
rg -n "defineStore|useSignalR|useDeviceId|FeatureName" web/src/stores web/src/composables web/src/types
```

Tests:

```powershell
rg -n "FeatureName|ControllerName|ServiceName|Fact|Theory" tests
```

## Documentation Update Rules

Always update documentation at the same granularity as the code change.

- Feature locator index: update `docs/feature-map/README.md` for any changed backend method, endpoint, frontend API method, route, page, state flow, or test evidence.
- Feature status: update `docs/requirements/feature-implementation-checklist.md` when a feature moves between `未实现`, `部分实现`, `待核验`, and `已实现`.
- API contract: update the matching `docs/contracts/*.md` when request paths, payloads, response fields, auth, or error behavior changes.
- Business rules: update the matching `docs/specs/spec-*.md` when validation, workflows, states, roles, or edge cases change.
- Frontend docs: update `docs/frontend/overview.md` or `docs/frontend/components.md` when routes, page ownership, shared components, stores, or UI conventions change.
- Data docs: update `docs/data/overview.md`, `docs/data/seed-data.md`, or `docs/sql/*.sql` when entities, migrations, seed data, or SQL fixtures change.
- Architecture docs: update `docs/architecture/overview.md` or `docs/architecture/decisions.md` only when the layering, technology choice, or cross-cutting pattern changes.

## Feature Map Row Standard

When adding or changing a row in `docs/feature-map/README.md`, include:

- stable feature IDs from the implementation checklist, or `NEW` until the requirement row exists;
- backend controller routes and service methods;
- frontend API methods and route/page/store/composable entry points;
- related specs, contracts, data docs, and tests;
- concise notes for ownership, verification gaps, or deferred third-party integrations.

Keep rows compact. Prefer filenames and method names over prose explanations.

## ByteBite Code Map

Backend layers:

- `src/ByteBite.Api/Controllers/*Controller.cs`: HTTP endpoints.
- `src/ByteBite.Application/Services/*Service.cs`: business logic and validation.
- `src/ByteBite.Infrastructure/Persistence/Entities/*.cs`: EF Core entities.
- `src/ByteBite.Infrastructure/Persistence/Migrations/*.cs`: schema changes.
- `src/ByteBite.Api/Hubs/*.cs` and `src/ByteBite.Api/Services/*Notification*.cs`: SignalR realtime behavior.
- `tests/ByteBite.UnitTests` and `tests/ByteBite.IntegrationTests`: verification evidence.

Frontend layers:

- `web/src/api/index.ts`: Axios instance and response unwrapping.
- `web/src/api/modules/*.ts`: API method wrappers.
- `web/src/router/index.ts`: route ownership.
- `web/src/pages/{customer,merchant,admin}/*.vue`: user-facing workflows.
- `web/src/stores/modules/*.ts`: shared state such as cart, merchant, and orders.
- `web/src/composables/*.ts`: device ID and SignalR helpers.
- `web/src/types/models/*.ts`: API-facing TypeScript models.

## Coordination With Checklist Skill

This skill answers "where is the feature implemented and documented?" The checklist skill answers "is the feature complete?" Use both for feature implementation work. Do not mark a feature complete from this locator alone.
