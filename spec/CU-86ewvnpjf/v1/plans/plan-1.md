# Plan 1: Fix Duplicate Reference Bug in Cyan Repositories

> Provide direction and suggestions, not exact code. Plans describe HOW to build, not the exact implementation.

## Goal

Fix the false `MultipleEntityNotFound` error when duplicate references are provided in batch entity fetch operations, and ensure output preserves input order and duplicates.

## Scope

### In Scope

- Fix deduplication comparison in all 4 repository batch fetch methods
- Modify output mapping to preserve input order and duplicates
- Update documentation in `docs/developer/features/` directory

### Out of Scope

- No unit or integration tests required
- No changes to error type or message format
- No changes to query logic (only deduplication and output mapping)

## Files to Modify

| File                                                           | Change Type | Notes                          |
| -------------------------------------------------------------- | ----------- | ------------------------------ |
| `App/Modules/Cyan/Data/Repositories/ResolverRepository.cs`     | modify      | Fix line 488 batch fetch logic |
| `App/Modules/Cyan/Data/Repositories/TemplateRepository.cs`     | modify      | Fix line 973 batch fetch logic |
| `App/Modules/Cyan/Data/Repositories/PluginRepository.cs`       | modify      | Fix line 480 batch fetch logic |
| `App/Modules/Cyan/Data/Repositories/ProcessorRepository.cs`    | modify      | Fix line 501 batch fetch logic |
| `docs/developer/features/<repository>-reference-resolution.md` | create      | Document batch fetch behavior  |

## Technical Approach

1. **Count distinct input refs:**

   - Use `{username, name, version}` tuple as key (version is null for versionless refs)
   - Example: `["a/b", "a/b", "a/c:1", "a/c:1", "d/e:3"]` → 3 distinct refs

2. **Query and build lookup** (keep existing logic for null-version → latest resolution):

   - Build a dictionary/map from found entities keyed by `{username, name, version}` (resolved versions)
   - Iterate through input refs in order, mapping each to the corresponding entity
   - This preserves both order and duplicates

3. **Check for missing refs:**

   - For each distinct input ref:
     - If version is null → check if ANY version exists for that `{username, name}` in the lookup
     - If version specified → check exact match in lookup
   - Collect missing refs into `notFound` set

4. **Apply the pattern consistently:**
   - All 4 repositories use the same approach
   - Keep existing grouping/query logic unchanged
   - Only change: how we count and compare, and how we detect missing refs
   - Reference types: `ResolverVersionRef`, `TemplateVersionRef`, `PluginVersionRef`, `ProcessorVersionRef`

## Edge Cases to Handle

- **All duplicates** (`["a/b", "a/b", "a/b"]`): Return `[entity, entity, entity]` (same entity 3 times), no error
- **Mixed duplicates with existing** (`["a/b", "c/d:1", "a/b"]`): Return `[entity1, entity2, entity1]` (preserve order)
- **Mixed duplicates with missing** (`["a/b", "x/y", "a/b"]` where `x/y` missing): Error with `notFound = ["x/y"]`
- **Different versions are distinct** (`["a/b:1", "a/b:2"]`): Requires both versions to exist
- **Versionless refs** (`["a/b"]`): Resolves to latest version
- **Empty input**: Preserve existing behavior

## How to Test

Since no tests are required:

1. Manual verification using the existing code flow
2. Verify `pre-commit run --all` passes
3. Verify `dotnet build` succeeds

## Integration Points

- **Depends on**: None
- **Blocks**: None
- **Shared state**: Each repository method is independent

## Implementation Checklist

- [ ] Fix ResolverRepository batch fetch to use lookup mapping and distinct comparison
- [ ] Fix TemplateRepository batch fetch to use lookup mapping and distinct comparison
- [ ] Fix PluginRepository batch fetch to use lookup mapping and distinct comparison
- [ ] Fix ProcessorRepository batch fetch to use lookup mapping and distinct comparison
- [ ] Update documentation in `docs/developer/features/` following current conventions
- [ ] Run `pre-commit run --all` and fix any issues
- [ ] Run `dotnet build` to verify compilation

## Success Criteria

- [ ] Input `["a/b", "a/b"]` returns `[entity, entity]` (same entity twice) when `a/b` exists
- [ ] Input `["a/b:1", "a/b:2"]` correctly requires both versions to exist
- [ ] Input `["a/b", "x/y", "a/b"]` where `x/y` missing returns error with `notFound = ["x/y"]`
- [ ] Input `["a/b", "a/c:1", "a/b"]` returns `[entity1, entity2, entity1]` (preserves order)
- [ ] `pre-commit run --all` passes without errors
