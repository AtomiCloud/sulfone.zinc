# Task Spec: Persist latest README metadata on entity push

**Ticket**: [86ex3vvy0](https://app.clickup.com/t/86ex3vvy0)
**Version**: 1

## Objective

When a user pushes a template, plugin, processor, or resolver, the incoming `readme` metadata must be persisted so subsequent top-level reads of that entity return the latest pushed README content rather than the original README content.

## Context

Today, push requests for registry entities include metadata with a `readme` field, but that field is not reliably persisted when pushing updates to an existing entity.

Current behavior:

- For existing entities, top-level fetches still return the initial README content.
- This makes push behave like version creation only, while the entity metadata shown at the top level becomes stale.

Observed service behavior in the current codebase:

- `TemplateService.Push()` already updates entity metadata before creating a new version.
- `PluginService.Push()`, `ProcessorService.Push()`, and `ResolverService.Push()` create the entity if it does not exist, but for existing entities they skip metadata update and only create a new version.

## Requirements

### Functional behavior

For all four entity types:

- template
- plugin
- processor
- resolver

When `POST /api/v1/{entity}/push/{username}` is called:

- If the entity does not exist yet, create it with the provided metadata, including `readme`.
- If the entity already exists, update its top-level metadata with the pushed metadata, including `readme`, before creating the new version.
- After a successful push, any endpoint that returns the top-level entity must expose the latest persisted `readme` value.

### Scope of persistence

The `readme` to persist is the entity metadata field, not a version-specific field.

That means:

- the latest push replaces the entity's current metadata `readme`
- top-level entity reads should show the most recently pushed README
- no separate versioned README history is required for this ticket unless it already exists implicitly through some other mechanism

### Affected push flows

The behavior must be consistent across:

1. `TemplateService.Push()`
2. `PluginService.Push()`
3. `ProcessorService.Push()`
4. `ResolverService.Push()`

### Read-path expectation

Any existing endpoint that returns the top-level entity representation should reflect the latest README automatically once the metadata update is persisted.

This ticket does **not** require redesigning response shapes. It requires the persisted metadata returned by current read paths to be up to date.

## Implementation expectations

Follow existing create/update patterns already present in the services and repositories.

Expected changes are likely in these areas:

1. Service-layer `Push()` logic for plugin, processor, and resolver so existing entities update metadata before version creation
2. Verification that template push already behaves correctly for `readme`
3. Any mapper/repository path if investigation shows `readme` is being dropped below the service layer
4. Tests covering repeated push behavior for existing entities

## Verification

Add or update tests to prove:

- pushing a new entity persists the provided `readme`
- pushing an existing entity with a different `readme` updates the top-level entity metadata
- after the second push, fetching the top-level entity returns the new `readme`, not the original one
- this behavior is covered for templates, plugins, processors, and resolvers, either with focused tests per entity or equivalent coverage at the right layer

## Constraints

- Do not introduce markdown file generation or `docs/README.md` regeneration
- Do not convert README into a versioned field unless the current model already requires that
- Preserve existing push/version creation behavior; this ticket only fixes stale top-level metadata
- Follow existing service/repository conventions in the codebase
- Commit message format: `fix: <description> [86ex3vvy0]`
- Do NOT commit — spec only

## Out of Scope

- Generating a registry catalog README
- Updating `docs/README.md`
- Rendering or transforming markdown content
- Changing API response contracts beyond what is necessary for current endpoints to surface the persisted latest `readme`
- Adding README diff/history features
