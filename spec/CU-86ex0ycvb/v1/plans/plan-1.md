# Plan 1: Add PresetAnswers to Template-Template Junction

> Provide direction and suggestions, not exact code. Plans describe HOW to build, not the exact implementation.

## Goal

Extend the template-template junction relationship to store and serve preset answer configs (`JsonElement`) per sub-template reference, following the established resolver config pattern. Values can be strings, arrays, booleans, or nested objects.

## Scope

### In Scope

- Database migration: add `PresetAnswers` column to `TemplateTemplateVersions` table
- Data layer: add `PresetAnswers` field to `TemplateTemplateVersionData`
- Domain layer: new `TemplateVersionTemplateRef` record (like `TemplateVersionResolverRef`)
- Domain service: new `TemplateLink` record (like `ResolverLink`), update `CreateVersion` signature
- Repository: update `CreateVersion` to persist preset answers, update `GetVersion` queries to include them
- API models: update `TemplateReferenceReq` with `PresetAnswers` (`JsonElement`), new `TemplateVersionTemplateRefResp` response
- API mapper: update request→domain and domain→response mappings
- Data mapper: update `ToDomain` on `TemplateVersionData` to map preset answers
- Unit tests for mappers and integration tests

### Out of Scope

- Iridium integration (push/inject preset answers)
- Validation of preset answer keys against sub-template prompts

## Files to Modify

| File                                                       | Change Type | Notes                                                                                                                  |
| ---------------------------------------------------------- | ----------- | ---------------------------------------------------------------------------------------------------------------------- |
| `App/Modules/Cyan/Data/Models/TemplateTemplateData.cs`     | modify      | Add `PresetAnswers` field (string, default `"{}"`)                                                                     |
| `Domain/Model/TemplateVersionTemplateRef.cs`               | create      | New domain record with `TemplateVersionPrincipal` + `JsonElement PresetAnswers`                                        |
| `Domain/Model/TemplateVersion.cs`                          | modify      | Change `Templates` from `IEnumerable<TemplateVersionPrincipal>` to `IEnumerable<TemplateVersionTemplateRef>`           |
| `Domain/Repository/ITemplateRepository.cs`                 | modify      | Add `TemplateLink` record, update `CreateVersion` to accept `IEnumerable<TemplateLink>` instead of `IEnumerable<Guid>` |
| `Domain/Service/TemplateService.cs`                        | modify      | Update `CreateVersion`/`Push` signatures and add `CreateTemplateLinks` helper                                          |
| `Domain/Service/ITemplateService.cs`                       | modify      | Update interface signatures                                                                                            |
| `Domain/Service/TemplateVersionTemplateInput.cs`           | create      | New input record (like `TemplateVersionResolverInput`)                                                                 |
| `App/Modules/Cyan/Data/Repositories/TemplateRepository.cs` | modify      | Update `CreateVersion` to persist `PresetAnswers`, update `GetVersion` `.Include()` paths                              |
| `App/Modules/Cyan/Data/Mappers/TemplateMapper.cs`          | modify      | Update `ToDomain` on `TemplateVersionData` to map preset answers                                                       |
| `App/Modules/Cyan/API/V1/Models/TemplateVersionModel.cs`   | modify      | Add `PresetAnswers` to `TemplateReferenceReq`, new `TemplateVersionTemplateRefResp`                                    |
| `App/Modules/Cyan/API/V1/Mappers/TemplateMapper.cs`        | modify      | Update `ToDomain` for `TemplateReferenceReq`, update `TemplateVersionResp` to use new response type                    |
| `App/Modules/Cyan/API/V1/Models/TemplateModel.cs`          | modify      | Check if `TemplateResp` references need updating                                                                       |
| `App/StartUp/Database/MainDbContext.cs`                    | modify      | No schema change needed (column already in data model), but verify `TemplateTemplateVersionData` config                |
| `App/Migrations/`                                          | create      | EF Core migration for new column                                                                                       |
| `UnitTest/TemplateVersionMapperTests.cs`                   | modify      | Add tests for preset answers mapping                                                                                   |

## Technical Approach

### Pattern Reference

Follow the resolver config pattern exactly:

**Resolver flow** (existing):

- `ResolverReferenceReq(Username, Name, Version, Config, Files)` → `TemplateVersionResolverInput(ResolverVersionRef, Config, Files)` → `ResolverLink(Guid, Config, Files)` → `TemplateResolverVersionData(Config, Files)` → `TemplateVersionResolverRef(Resolver, Config, Files)` → `TemplateVersionResolverResp`

**Sub-template flow** (new):

- `TemplateReferenceReq(Username, Name, Version, PresetAnswers)` → `TemplateVersionTemplateInput(TemplateVersionRef, PresetAnswers)` → `TemplateLink(Guid, PresetAnswers)` → `TemplateTemplateVersionData(PresetAnswers)` → `TemplateVersionTemplateRef(Template, PresetAnswers)` → `TemplateVersionTemplateRefResp` (all `PresetAnswers` as `JsonElement`)

### Steps

1. **Data layer** — Add `PresetAnswers` to `TemplateTemplateVersionData` as `string` with default `"{}"`
2. **Migration** — Run `direnv exec . pls migration:create -- AddPresetAnswersToTemplateTemplateVersions` to generate EF Core migration adding non-nullable `PresetAnswers` column with default `'{}'`
3. **Domain types** — Create `TemplateVersionTemplateRef` and `TemplateVersionTemplateInput` records
4. **Domain model** — Update `TemplateVersion.Templates` type from `IEnumerable<TemplateVersionPrincipal>` to `IEnumerable<TemplateVersionTemplateRef>`
5. **Repository interface** — Add `TemplateLink` record, change `CreateVersion` template parameter from `IEnumerable<Guid>` to `IEnumerable<TemplateLink>`
6. **Repository implementation** — Update `CreateVersion` to serialize `PresetAnswers` from `TemplateLink` into `TemplateTemplateVersionData`
7. **Repository queries** — No `.Include()` changes needed (already includes `TemplateRefs` → `TemplateRef`)
8. **Service layer** — Update `CreateVersion`/`Push` signatures, add `CreateTemplateLinks` helper (position-based matching like `CreateResolverLinks`)
9. **API models** — Add `PresetAnswers: JsonElement` to `TemplateReferenceReq`, create `TemplateVersionTemplateRefResp`
10. **API mapper** — Update request → domain chain and domain → response chain
11. **Data mapper** — Update `TemplateVersionData.ToDomain()` to construct `TemplateVersionTemplateRef` with deserialized preset answers
12. **Tests** — Update existing tests, add new mapper tests for preset answers
13. **Pre-commit** — Ensure all commits pass pre-commit hooks (treefmt, gitlint, etc.)
14. **Dev server verification** — Start dev server with `direnv exec . pls dev`, wait for it to be up, then test the endpoints manually (create template version with preset answers, read back and verify)
15. **Documentation** — Update relevant docs to reflect the new API shape (preset answers on sub-template references)

### Data Serialization

- Store: `JsonSerializer.Serialize(PresetAnswers)` → `string` in PostgreSQL TEXT column
- Load: `JsonSerializer.Deserialize<JsonElement>(PresetAnswers)` → `JsonElement`
- API request: `JsonElement` (deserialize from JSON body — values can be string, array, bool, or object)
- API response: `JsonElement` (serialize to JSON)

### Position-Based Matching

Same as resolver links (PR #49): match sub-template inputs to resolved versions by array index, not by identity. This allows duplicate sub-template references with different preset answers.

## Edge Cases to Handle

- **Null preset answers on read**: If `PresetAnswers` column is somehow null (legacy data), fall back to empty dictionary
- **Empty presets**: `{}` is valid — means no preset answers for this sub-template
- **Serialization round-trip**: Ensure `JsonElement` serializes to and deserializes from JSON consistently

## How to Test

1. Unit test: serialize `JsonElement` (with various value types: string, array, bool) to JSON string and back — verify round-trip
2. Unit test: map `TemplateReferenceReq` with `PresetAnswers` through `ToDomain()` → verify `TemplateVersionTemplateInput` has correct values
3. Unit test: map `TemplateVersionTemplateRef` through API mapper → verify `TemplateVersionTemplateRefResp` has correct values
4. Update existing `TemplateVersionMapperTests` to account for new `TemplateVersion.Templates` type
5. Build: `direnv exec . pls build`
6. Test: `direnv exec . pls test`
7. Start dev server: `direnv exec . pls dev`, wait for server to be up
8. Manual endpoint test: create a template version with sub-template references including `PresetAnswers`, then GET the version and verify preset answers are returned correctly
9. Verify pre-commit hooks pass on all commits

## Integration Points

- **Depends on**: Nothing (self-contained Zinc change)
- **Blocks**: Ir task "Push preset configs + inject into dependency tree resolution" (86ex0ybx9 sibling)
- **Shared state**: The `TemplateVersion` domain model shape changes — consumers that read `Templates` will now get `TemplateVersionTemplateRef` instead of bare `TemplateVersionPrincipal`

## Implementation Checklist

- [ ] Code changes per approach above
- [ ] Migration generated via `direnv exec . pls migration:create -- AddPresetAnswersToTemplateTemplateVersions`
- [ ] Linting passes (`direnv exec . pls build`)
- [ ] Unit tests added/updated (`direnv exec . pls test`)
- [ ] Pre-commit hooks pass on all commits
- [ ] Dev server started (`direnv exec . pls dev`), endpoints tested manually
- [ ] No regressions in existing functionality
- [ ] Documentation updated to reflect new API shape

## Success Criteria

- [ ] `TemplateTemplateVersions` table has `PresetAnswers` column with default `{}`
- [ ] API accepts `PresetAnswers` on sub-template references
- [ ] API returns `PresetAnswers` in sub-template response type
- [ ] All existing tests pass
- [ ] New mapper tests cover preset answers serialization (including non-string values: arrays, bools)
- [ ] Dev server runs and endpoints verified manually
- [ ] Pre-commit hooks pass on all commits
- [ ] Documentation updated
