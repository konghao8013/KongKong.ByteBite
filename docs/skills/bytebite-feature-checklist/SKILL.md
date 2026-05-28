---
name: bytebite-feature-checklist
description: Maintain the KongKong.ByteBite feature implementation checklist. Use when Codex works on ByteBite requirements, feature implementation, bug fixes, UI changes, backend/API changes, verification, or any task that may change whether a requirement is implemented.
---

# ByteBite Feature Checklist

Use this skill for KongKong.ByteBite work that touches product requirements or implementation status.

## Required Workflow

1. Read `docs/requirements/requirements-overview.md` for the intended behavior.
2. Read `docs/requirements/feature-implementation-checklist.md` before changing code or requirements.
3. If the user adds a new requirement, add or update checklist rows first with status `未实现` unless there is verified existing implementation.
4. When implementing a feature, update the matching checklist row in the same turn:
   - use `部分实现` when only part of the stack is complete;
   - use `待核验` when code exists but has not been verified;
   - use `已实现` only after backend, frontend, data flow, and verification evidence match the requirement.
5. Keep feature IDs stable. Do not delete rows for removed or deferred scope; mark the reason in `待补齐/备注`.
6. Add concise evidence in `当前依据`, such as controller/service/page/test filenames.
7. Update `最后盘点日期` when doing a broad checklist review.
8. Treat every `部分实现` and `未实现` checklist item as implementation work to finish in the current scope unless it depends on paid third-party services.

## Status Rules

- `已实现`: user-facing workflow is present and verified against the requirement.
- `部分实现`: some required layers exist, but key UI/API/data/test behavior is missing.
- `未实现`: no meaningful implementation was found.
- `待核验`: implementation appears present, but verification is pending or inconclusive.
- Third-party paid capabilities, including SMS verification/notification and online payment provider integrations, are not required for the current implementation scope. Mark the in-scope fallback or configuration as evidence and explain the deferred paid integration in `待补齐/备注`; do not leave these as ordinary implementation gaps.

## Guardrails

- Do not mark a feature complete from memory. Inspect the relevant files first.
- Do not count generated `web/dist` files as source evidence.
- Prefer conservative status. If frontend and backend disagree, mark `部分实现`.
- When a bug fix completes an existing feature, update the row status and evidence.
- When adding tests for an implemented feature, update the evidence field if that changes confidence.
