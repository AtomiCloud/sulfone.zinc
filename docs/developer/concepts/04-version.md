# Version Concept

**What**: Auto-incrementing `ulong` version numbers for registry entities.
**Why**: Enables immutable version tracking and dependency resolution.

**Important**: Previous documentation incorrectly described versions as semver-encoded.

## Actual Version Format

- **Type**: `ulong` (simple integer)
- **Pattern**: Auto-increment starting from 1
- **Uniqueness**: Unique per parent entity (Id + Version)
- **Immutability**: Once created, versions cannot be modified

**Example**:

```
my-template: v1, v2, v3, v4, ...
```

**NOT (Previous Incorrect Docs)**:

- ❌ `major*256^2 + minor*256 + patch` - Semver encoding was never implemented
- ❌ Semantic versioning - Versions are simple incrementing integers

## Version Structure

All versioned entities follow the same structure:

```mermaid
flowchart TB
    subgraph Version Entity
        Id[Guid Id]
        Version[ulong Version]
        ParentId[Parent GUID]
        Record[Record Fields]
        Property[Optional Properties]
    end

    subgraph Record Fields
        DockerImage[Docker Image]
        DockerTag[Docker Tag]
    end

    subgraph Dependencies
        Processors[Processor Versions]
        Plugins[Plugin Versions]
        Templates[Template Versions]
    end

    Version -->|References| Dependencies
```

## Version Models

| Entity    | Version Data Model     | Key File                                               |
| --------- | ---------------------- | ------------------------------------------------------ |
| Template  | `TemplateVersionData`  | `App/Modules/Cyan/Data/Models/TemplateVersionData.cs`  |
| Processor | `ProcessorVersionData` | `App/Modules/Cyan/Data/Models/ProcessorVersionData.cs` |
| Plugin    | `PluginVersionData`    | `App/Modules/Cyan/Data/Models/PluginVersionData.cs`    |

## Version Creation Flow

```mermaid
sequenceDiagram
    participant Client
    participant Service
    participant ProcessorRepo
    participant PluginRepo
    participant TemplateRepo
    participant DB

    Client->>Service: CreateVersion(..., processors, plugins, templates)
    Service->>ProcessorRepo: GetAllVersion(processors)
    ProcessorRepo->>DB: SELECT WHERE id IN (...)
    DB-->>ProcessorRepo: Processor Versions
    ProcessorRepo-->>Service: Processor IDs

    Service->>PluginRepo: GetAllVersion(plugins)
    PluginRepo->>DB: SELECT WHERE id IN (...)
    DB-->>PluginRepo: Plugin Versions
    PluginRepo-->>Service: Plugin IDs

    Service->>TemplateRepo: GetAllVersion(templates)
    TemplateRepo->>DB: SELECT WHERE id IN (...)
    DB-->>TemplateRepo: Template Versions
    TemplateRepo-->>Service: Template IDs

    Service->>TemplateRepo: CreateVersion(..., processorIds, pluginIds, templateIds)
    TemplateRepo->>DB: INSERT INTO TemplateVersions
    DB-->>TemplateRepo: New Version (auto-incremented)
    TemplateRepo-->>Service: TemplateVersionPrincipal
    Service-->>Client: Result
```

**Key File**: `Domain/Service/TemplateService.cs:160-196`

## Version References

Versions can reference specific versions of other entities:

```mermaid
flowchart TB
    TemplateVer[Template Version] -->|References| ProcessorVers[Processor Versions]
    TemplateVer -->|References| PluginVers[Plugin Versions]
    TemplateVer -->|References| TemplateVers[Other Template Versions]
```

### Reference Tables

| From             | To                | Junction Table          |
| ---------------- | ----------------- | ----------------------- |
| Template Version | Processor Version | `TemplateProcessorData` |
| Template Version | Plugin Version    | `TemplatePluginData`    |
| Template Version | Template Version  | `TemplateTemplateData`  |

## Version Resolution

### Get Latest Version

```mermaid
flowchart TB
    Query[Query by Parent] --> OrderBy[ORDER BY Version DESC]
    OrderBy --> First[Take First]
    First --> Result[Latest Version]
```

**Key File**: `App/Modules/Cyan/Data/Repositories/TemplateRepository.cs:370-400`

### Get Specific Version

```mermaid
flowchart TB
    Query[Query by Parent + Version] --> Filter[WHERE Version = X]
    Filter --> Single[SingleOrDefault]
    Single --> Result[Specific Version]
```

**Key File**: `App/Modules/Cyan/Data/Repositories/TemplateRepository.cs:95-122`

## Version Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Created: CreateVersion()
    Created --> Downloaded: GetVersion(bumpDownload=true)
    Created --> Updated: UpdateVersion()
    Updated --> Downloaded: GetVersion(bumpDownload=true)

    note right of Downloaded
        Increments download count
    end note
```

## Download Tracking

Versions track download counts:

```mermaid
flowchart TB
    GetVersion[GetVersion] --> CheckBump{bumpDownload?}
    CheckBump -->|Yes| Increment[IncrementDownload]
    CheckBump -->|No| Return[Return Version]
    Increment --> Return
```

**Key File**: `Domain/Service/TemplateService.cs:82-115`

## Version Properties

Optional properties can be attached to versions:

| Property      | Type   | Purpose                   |
| ------------- | ------ | ------------------------- |
| `Description` | string | Version-specific notes    |
| `DockerImage` | string | Container image reference |
| `DockerTag`   | string | Container image tag       |

**Key File**: `Domain/Model/TemplateVersion.cs` → `TemplateVersionProperty`

## Related Concepts

- [Registry](./03-registry.md) - Parent container entities
- [Dependency](./05-dependency.md) - Cross-version references
- [Dependency Resolution Algorithm](../algorithms/01-dependency-resolution.md) - Implementation details
- [Version Resolution Algorithm](../algorithms/02-version-resolution.md) - Query patterns
