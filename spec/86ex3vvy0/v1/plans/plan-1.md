# Plan 1: Persist Latest README Atomically During Push

## Goal

Make push update top-level metadata for existing templates, plugins, processors, and resolvers so the latest pushed `readme` is persisted atomically with version creation.

## Scope

- Update push behavior for plugin, processor, and resolver so existing entities refresh metadata before creating a version
- Verify template push still behaves correctly for README persistence
- Ensure metadata update + version creation happen as one atomic write
- Keep current API contracts unchanged
- Keep README as top-level metadata, not version metadata
- Add only light regression coverage at the relevant layer

## Core Requirement

A push that updates metadata and creates a new version must behave atomically:

- if both steps succeed, the latest top-level `readme` is visible and the new version exists
- if either step fails, neither partial metadata update nor partial version creation should remain persisted

Use the project transaction abstraction if one exists for this flow. The repository already contains explicit transaction handling in `App/Modules/Cyan/Data/Repositories/TemplateRepository.cs:599` and `App/Modules/Cyan/Data/Repositories/TemplateRepository.cs:754`; the implementation for this ticket should follow the same atomic-write principle via `ITransactionManager` if available, otherwise by the repo’s established transaction-management pattern.

## Files to Modify

### 1. `Domain/Service/PluginService.cs`

Update `Push()` so existing entities do not short-circuit directly to the current principal.

Current behavior:

- if plugin exists, return `p.Principal`
- then create a new version

Required behavior:

- if plugin exists, update top-level metadata with the pushed metadata, including `readme`
- then create the new version as part of the same atomic unit

Expected effect:

- top-level plugin metadata, including `readme`, reflects the latest push
- no partial write if version creation fails after metadata update

### 2. `Domain/Service/ProcessorService.cs`

Apply the same change as plugin push.

Current behavior:

- if processor exists, return `p.Principal`
- then create a new version

Required behavior:

- if processor exists, update top-level metadata with the pushed metadata, including `readme`
- then create the new version as part of the same atomic unit

### 3. `Domain/Service/ResolverService.cs`

Apply the same change as plugin and processor push.

Current behavior:

- if resolver exists, return `p.Principal`
- then create a new version

Required behavior:

- if resolver exists, update top-level metadata with the pushed metadata, including `readme`
- then create the new version as part of the same atomic unit

### 4. `Domain/Service/TemplateService.cs`

Verify template push remains correct.

Current behavior:

- existing templates already call `repo.Update(username, pRecord.Name, metadata)` before creating a version

Required behavior:

- confirm template push also participates in the same atomic write boundary
- adjust only if investigation shows template metadata update and version creation are not actually atomic together

### 5. Repository / transaction boundary

Investigate where the atomic boundary should live.

The likely implementation options are:

- wrap metadata update + version creation inside a shared transaction manager abstraction if the codebase already provides one under a different name than the exact interface searched
- otherwise move or coordinate the writes so they execute inside a single repository-level transaction following the established EF transaction pattern already present in template version creation

Important requirement:

- do not leave service-level sequencing as two independent committed writes

## Design Notes

- The persisted `readme` is part of top-level entity metadata, not version metadata.
- The latest push wins for metadata fields.
- This ticket fixes stale metadata on existing entities.
- Atomicity matters because push is logically one write operation, not two unrelated mutations.
- No response-shape redesign is needed if persistence is corrected.

## Light Verification

Add only focused regression coverage at the most appropriate existing layer.

Minimum useful proof:

- repeated push updates top-level `readme` for the affected entity flows
- no overly broad e2e expansion
- test only enough to lock the regression and atomic behavior expectation in place

## Edge Cases

- Pushing a brand new entity should still create it with the incoming metadata.
- Pushing an existing entity with an unchanged `readme` should still succeed and create the new version.
- Pushing an existing entity with a changed `readme` should replace the top-level persisted `readme`.
- If version creation fails, the metadata update must not remain committed by itself.
- If metadata update fails, version creation must not proceed.

## Implementation Checklist

- [ ] Identify the transaction abstraction to use for atomic push writes (`ITransactionManager` if present, or the repo’s established equivalent)
- [ ] `PluginService.Push()` path updates metadata for existing plugin before version creation within one atomic write
- [ ] `ProcessorService.Push()` path updates metadata for existing processor before version creation within one atomic write
- [ ] `ResolverService.Push()` path updates metadata for existing resolver before version creation within one atomic write
- [ ] `TemplateService.Push()` verified to participate in the same atomic behavior, or adjusted if needed
- [ ] Add light regression coverage for latest README persistence on repeated push
- [ ] No changes to request/response shapes
- [ ] No changes to README semantics beyond fixing persistence

## Non-Goals

- `docs/README.md` generation
- markdown rendering/transformation
- versioned README history
- broad e2e coverage
- schema redesign unless investigation proves the current persistence layer cannot store updated metadata correctly
