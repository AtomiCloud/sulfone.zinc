# Task Spec: Store post-creation command configs in registry

**Ticket**: [86ex0yctm](https://app.clickup.com/t/86ex0yctm)
**Version**: 1

## Objective

Add a `Commands` field to every template version. Commands are an array of strings — one string is one shell command, no args structure. When a client pushes/creates a template version, they provide commands (at least an empty array). When a client queries a template version, they receive the commands array.

## Context

This is the Zinc (registry) side of the "Post-creation commands" feature. The client (Iridium) will:

- Parse commands from `cyan.yaml` and push them to Zinc during template publish
- Retrieve commands from Zinc during template execution and run them on the user's machine

Zinc's responsibility is purely storage and retrieval — no command execution, no validation of command content beyond type checks.

## Requirements

### Data Model

- Add `string[] Commands` column to `TemplateVersionData` (EF Core entity)
- PostgreSQL type: `text[]`
- Default value: empty array (`[]`) — existing rows get `[]` during migration
- This is a per-version field — each version of a template can have different commands

### API Endpoints

All endpoints that create or return template versions must support the new field:

1. **Create version** (`POST /api/v1/template/slug/{username}/{name}/versions`)

   - Accept `string[] Commands` in request body (required field, defaults to `[]`)
   - Store in `TemplateVersionData`

2. **Push template** (`POST /api/v1/template/push/{username}`)

   - Accept `string[] Commands` in the version portion of the push payload (required field, defaults to `[]`)
   - Store in `TemplateVersionData`

3. **Get version** (`GET /api/v1/template/slug/{username}/{name}/versions/{ver}`)

   - Return `string[] Commands` in response (required field, always present)

4. **Get template by slug/id** (endpoints that return full template data)
   - Include `Commands` in all version response models (required field, always present)
   - Template references within a version (nested template version data) must also include `Commands` — the full response tree carries commands at every level

### Validation

- Commands must be a non-null array of strings
- Each command string must not be null or empty/whitespace
- No validation of command content (shell syntax, injection, etc.) — that is the client's responsibility

### Layers to Update

Following the existing pattern used for resolver `Config`/`Files` fields. **Scan ALL response models related to template versions** — every model that represents or references a template version must include `Commands`:

1. **Migration**: `pls migration:create -- AddCommandsToTemplateVersions`
2. **Data model**: `TemplateVersionData` — add `string[] Commands` property
3. **Domain model**: `TemplateVersionPrincipal` — add `string[] Commands` so it's included everywhere a version appears (top-level and nested in template references)
4. **API request/response models**: Scan and update ALL records that carry template version data — add `string[] Commands` as a required field
5. **Mappers**: API↔Domain and Data↔Domain mappers — propagate Commands through all mapping paths
6. **Validators**: FluentValidation rule for `string[] Commands`
7. **Service/Repository**: Propagate through `CreateVersion` and all query methods

### Verification

- Run `pls dev` and test local endpoints to confirm Commands is returned in all relevant responses
- Run `pre-commit run --all` and ensure all checks pass

## Constraints

- **Breaking change**: All response models require `string[] Commands` — clients must be updated to handle the new field
- Follow existing codebase patterns (records, FluentAssertions tests, FluentValidation)
- Commit message format: `feat: <description> [CU-86ex0yctm]`
- Do NOT commit — implementation only

## Out of Scope

- Command execution (handled by Iridium)
- Parsing cyan.yaml (handled by Iridium)
- Command validation beyond type/emptiness checks
- Any changes to template, plugin, processor, or resolver models
