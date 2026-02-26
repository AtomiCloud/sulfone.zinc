# Dependency Resolution Algorithm

**Used by**:

- [Template Version Creation](../features/03-template-registry.md)

## Overview

<!--
NOTE: The phrasing "ensures template versions never have broken dependencies" refers to direct reference validation.
Self-references and circular references are not detected - this is documented in the Edge Cases section below.
-->

Validates that all directly referenced processor, plugin, and template versions exist before creating a new template version. This ensures template versions have no unresolvable direct dependencies, though self-references and circular references are not detected (see Edge Cases).

## Input

| Parameter    | Type                               | Description                  |
| ------------ | ---------------------------------- | ---------------------------- |
| `processors` | `IEnumerable<ProcessorVersionRef>` | Processor version references |
| `plugins`    | `IEnumerable<PluginVersionRef>`    | Plugin version references    |
| `templates`  | `IEnumerable<TemplateVersionRef>`  | Template version references  |

Each reference contains:

- `Username` (string) - Owner's username
- `Name` (string) - Entity name
- `Version` (ulong?) - Optional specific version number (null = latest)

## Output

| Result    | Description                                 |
| --------- | ------------------------------------------- |
| `Success` | All dependencies validated, version created |
| `Error`   | One or more dependencies not found          |

## Steps

```mermaid
sequenceDiagram
    participant Client
    participant Service as TemplateService
    participant PluginRepo as PluginRepository
    participant ProcessorRepo as ProcessorRepository
    participant TemplateRepo as TemplateRepository
    participant DB as Database

    Client->>Service: CreateVersion(userId, name, record, ...)
    Note over Service: Fetch all dependency versions (eager evaluation)

    Service->>PluginRepo: GetAllVersion(plugins)
    PluginRepo->>DB: SELECT * FROM PluginVersions WHERE Id IN (...)
    DB-->>PluginRepo: Results
    PluginRepo-->>Service: Result<IEnumerable<PluginVersion>>

    Service->>ProcessorRepo: GetAllVersion(processors)
    ProcessorRepo->>DB: SELECT * FROM ProcessorVersions WHERE Id IN (...)
    DB-->>ProcessorRepo: Results
    ProcessorRepo-->>Service: Result<IEnumerable<ProcessorVersion>>

    Service->>TemplateRepo: GetAllVersion(templates)
    TemplateRepo->>DB: SELECT * FROM TemplateVersions WHERE Id IN (...)
    DB-->>TemplateRepo: Results
    TemplateRepo-->>Service: Result<IEnumerable<TemplateVersion>>

    Note over Service: Combine results via LINQ monadic comprehension

    alt All Dependencies Found
        Service->>TemplateRepo: CreateVersion(..., ids)
        TemplateRepo->>DB: INSERT with dependency IDs
        DB-->>TemplateRepo: Success
        TemplateRepo-->>Service: Success
        Service-->>Client: Success: TemplateVersionPrincipal
    else Any Dependency Missing
        Service-->>Client: Error: Dependency not found
    end
```

**Key File**: `Domain/Service/TemplateService.cs:160-196`

## Detailed Logic

```csharp
public async Task<Result<TemplateVersionPrincipal?>> CreateVersion(
    string userId,
    string name,
    TemplateVersionRecord record,
    TemplateVersionProperty? property,
    IEnumerable<ProcessorVersionRef> processors,
    IEnumerable<PluginVersionRef> plugins,
    IEnumerable<TemplateVersionRef> templates
)
{
    // Validate all dependencies exist
    var pluginResults = await plugin.GetAllVersion(plugins);
    var processorResults = await processor.GetAllVersion(processors);
    var templateResults = await repo.GetAllVersion(templates);

    // Combine results using LINQ
    // NOTE: The LINQ range variables (plugin, processor, template) intentionally shadow the injected
    // repository fields (IPluginRepository plugin, IProcessorRepository processor, etc.) within the query scope.
    // This is safe because the fields are accessed before the query (via GetAllVersion) and the range variables
    // are only used within the LINQ expression.
    var a = from plugin in pluginResults
            from processor in processorResults
            from template in templateResults
            select (plugin.Select(x => x.Id), processor.Select(x => x.Id), template.Select(x => x.Id));

    return await a
        .ThenAwait(refs =>
        {
            var (pluginIds, processorIds, templateIds) = refs;
            return repo.CreateVersion(userId, name, record, property, processorIds, pluginIds, templateIds);
        });
}
```

**Key File**: `Domain/Service/TemplateService.cs:160-196`

## Edge Cases

| Case               | Input                      | Behavior                              | Key File                              |
| ------------------ | -------------------------- | ------------------------------------- | ------------------------------------- |
| Empty dependencies | `[]`, `[]`, `[]`           | Version created without dependencies  | `TemplateService.cs:160-196`          |
| Missing processor  | Invalid processor ID       | Returns error before creating version | `ProcessorRepository.GetAllVersion()` |
| Missing plugin     | Invalid plugin ID          | Returns error before creating version | `PluginRepository.GetAllVersion()`    |
| Missing template   | Invalid template ID        | Returns error before creating version | `TemplateRepository.GetAllVersion()`  |
| Self-reference     | Template references itself | Not explicitly prevented              | N/A                                   |
| Circular reference | A→B→A                      | Not explicitly prevented              | N/A                                   |

> **Note**: Self-referencing template versions and circular dependencies (A→B→A) will silently persist in the database since this algorithm only validates direct references without graph traversal. These can cause infinite loops in future graph-traversal tooling (e.g., dependency resolution at install time). Consider adding pre-CreateVersion validation to detect self-references and simple cycles before persisting.

<!-- TODO: Implement cycle detection in CreateVersion flow before adding graph-traversal consumers -->

## Error Handling

| Error            | Cause                            | Handling                           |
| ---------------- | -------------------------------- | ---------------------------------- |
| `EntityNotFound` | Referenced version doesn't exist | Returns error, version not created |
| Database error   | Query failure                    | Returns error, version not created |

## Complexity

| Aspect    | Complexity                                              |
| --------- | ------------------------------------------------------- |
| **Time**  | O(n + m + p) where n=plugins, m=processors, p=templates |
| **Space** | O(n + m + p) for loading all versions                   |

## Related

- [Dependency Concept](../concepts/05-dependency.md) - Dependency relationships
- [Template Version Creation](../features/03-template-registry.md) - Feature usage
