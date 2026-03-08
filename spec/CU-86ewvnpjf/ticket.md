# Ticket: CU-86ewvnpjf

- **Type**: Bug
- **Status**: todo
- **URL**: https://app.clickup.com/t/86ewvnpjf
- **Parent**: none
- **Assignee**: Adelphi Liong (adelphi@atomi.cloud)
- **Priority**: normal

## Description

Problem

The repository methods that fetch multiple entities by reference have a bug where they fail when duplicate references are provided in the input array.

Root Cause

The code compares result.Length != inputRefs.Length after grouping results. Since grouping removes duplicates, if the input contains duplicate references, the lengths won't match even if all unique entities were found.

Example

Input: ["cyane2e/resolver1:1", "cyane2e/resolver1:1"] (duplicate)

After query and grouping: 1 unique resolver found
Check: 1 != 2 -> incorrectly triggers MultipleEntityNotFound error

Affected Files

All 4 repositories in App/Modules/Cyan/Data/Repositories/ have the same bug:

| File                   | Line | Method                                             |
| ---------------------- | ---- | -------------------------------------------------- |
| ResolverRepository.cs  | 488  | Compares resolvers.Length != resolverRefs.Length   |
| TemplateRepository.cs  | 973  | Compares templates.Length != templateRefs.Length   |
| PluginRepository.cs    | 480  | Compares plugins.Length != pluginRefs.Length       |
| ProcessorRepository.cs | 501  | Compares processors.Length != processorRefs.Length |

Suggested Fix

Compare against distinct references instead:

```
var distinctRefs = resolverRefs.DistinctBy(x => $"{x.Username}/{x.Name}:{x.Version}").Count();
if (resolvers.Length != distinctRefs)
{
  // ... error handling
}
```

Acceptance Criteria

- Fix all 4 affected repository methods
- Add unit tests that verify duplicate references don't cause false "not found" errors
- Verify existing tests still pass

## Comments

No comments on this ticket.
