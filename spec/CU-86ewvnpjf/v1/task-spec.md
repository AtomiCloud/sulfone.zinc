# Task Specification: [Bug] MultipleEntityNotFound false positive when input contains duplicate references (CU-86ewvnpjf)

## Source

- Ticket: CU-86ewvnpjf
- System: ClickUp
- URL: https://app.clickup.com/t/86ewvnpjf

## Summary

Fix a bug in 4 repository methods (ResolverRepository, TemplateRepository, PluginRepository, ProcessorRepository) where duplicate references in the input array cause a false `MultipleEntityNotFound` error. The methods must (1) correctly identify only truly missing references by comparing against distinct input refs, and (2) preserve duplicates in the output to match input length and order.

## Acceptance Criteria

- [ ] Fix ResolverRepository.cs line 488 to compare against distinct references
- [ ] Fix TemplateRepository.cs line 973 to compare against distinct references
- [ ] Fix PluginRepository.cs line 480 to compare against distinct references
- [ ] Fix ProcessorRepository.cs line 501 to compare against distinct references
- [ ] Output array must preserve duplicates matching input length and order (e.g., input `["ref1", "ref1"]` returns `[entity1, entity1]`)
- [ ] Error handling must correctly identify only truly missing references (not duplicates)

## Out of Scope

- No unit or integration tests to be added
- No changes to error type or error message format
- No changes to overall query logic (only the deduplication and length check logic)

## Constraints

- Deduplication key for counting: `{username, name, version}` (null version for versionless refs)
- Null-version refs (`a/b`) resolve to latest version at query time
- For missing detection: null-version refs check if ANY version exists; versioned refs check exact match
- Input order must be preserved in output
- Must not break existing functionality for non-duplicate inputs

## Context

The bug occurs because the code compares `result.Length != inputRefs.Length` after grouping results. Since grouping removes duplicates by `{name, username}` (not including version), if the input contains duplicate references, the lengths won't match even if all entities exist.

**Example of current bug:**

- Input: `["cyane2e/resolver1:1", "cyane2e/resolver1:1"]` (duplicate)
- After query and grouping: 1 unique resolver found
- Check: `1 != 2` → incorrectly triggers `MultipleEntityNotFound`

**Fixed approach:**

1. Count distinct input refs by `{username, name, version}` (null for versionless)
2. Query and resolve null-version refs to latest (existing logic)
3. Build lookup of found entities keyed by `{username, name, version}` (resolved versions)
4. For each distinct input ref:
   - If version is null → check if ANY version exists for that `{username, name}`
   - If version specified → check exact match
5. If any missing → throw `MultipleEntityNotFound` with missing refs
6. Map input refs to output entities preserving order and duplicates

**Example of correct behavior after fix:**

- Input: `["a/b", "a/b", "a/c:1", "a/c:1", "d/e:3"]`
- Distinct refs: 3 (`a/b`, `a/c:1`, `d/e:3`)
- Lookup count: 3 (after resolving `a/b` to latest)
- Check: `3 == 3` → success, returns 5 entities (preserving duplicates)

**Error case after fix:**

- Input: `["a/b", "a/b", "x/y"]` where `x/y` doesn't exist
- Distinct refs: 2 (`a/b`, `x/y`)
- Lookup count: 1 (only `a/b` found)
- Check: `1 != 2` → `notFound = ["x/y"]`, returns `MultipleEntityNotFound`

## Edge Cases

- **All duplicates** (`["a/b", "a/b", "a/b"]`): Should return `[entity, entity, entity]` (same entity 3 times), no error
- **Mixed duplicates with existing** (`["a/b", "c/d:1", "a/b"]`): Should return `[entity1, entity2, entity1]` (preserve order)
- **Mixed duplicates with missing** (`["a/b", "x/y", "a/b"]` where `x/y` missing): Should return error with `notFound = ["x/y"]`
- **Different versions are distinct** (`["a/b:1", "a/b:2"]`): Requires both versions to exist, both are returned
- **Versionless refs** (`["a/b"]` where version is null): Resolves to latest version, returned entity has resolved version
- **Empty input**: Edge case - depends on existing behavior, preserve current handling

---

## Domain-Driven Design (Optional)

_Only include if DDD skill exists in the repository. Checked during planning phase._

### Bounded Context(s)

- **Primary Context:** Data Persistence — Repository layer for entity fetching and batch operations in the Cyan module

### Ubiquitous Language

| Term                   | Definition                                                    |
| ---------------------- | ------------------------------------------------------------- |
| **Reference**          | Entity reference in format `username/name:version`            |
| **Distinct Reference** | Unique reference based on `{username, name, version}`         |
| **Resolvers**          | Entity type in ResolverRepository with username/name/version  |
| **Templates**          | Entity type in TemplateRepository with username/name/version  |
| **Plugins**            | Entity type in PluginRepository with username/name/version    |
| **Processors**         | Entity type in ProcessorRepository with username/name/version |

## Implementation Checklist

**Ensure all relevant skills in skill folders are applied to this implementation.**

### Documentation

- [ ] Update documentation in `docs/` directory following current conventions
- [ ] No inline comments required
- [ ] No README changes (internal bug fix)

### Linting

- [ ] Run `pre-commit run --all`
- [ ] Fix all linting errors before committing

### Notes

- Each repository method has the same pattern bug - fix consistently across all 4 files
- **Key fix**: Count distinct input refs by `{username, name, version}` (null for versionless) and compare against lookup count
- For missing detection: null-version refs check ANY version exists; versioned refs check exact match
- Output mapping: iterate input refs in order, lookup each entity (preserves order and duplicates)
- Keep existing null-version → latest resolution logic unchanged
