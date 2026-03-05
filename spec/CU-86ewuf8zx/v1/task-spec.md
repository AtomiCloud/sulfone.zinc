# Task Specification: [Zn] Add resolver config and glob patterns to registry API models (CU-86ewuf8zx)

## Summary

Extend the Zinc registry API to include resolver configuration data in the `TemplateVersionResp` model. This enables Iridium to build the ResolverRegistry during VFS layering by reading resolver configs (including glob patterns) from the template version response.

**Ticket:** CU-86ewuf8zx | **System:** ClickUp | **URL:** https://app.clickup.com/t/86ewuf8zx
**Parent:** CU-86ewr9nen (Resolver system) | **Blocked by:** CU-86ewrbrfc (Resolver registry system)

---

## Current State Analysis

### Existing Implementation (from CU-86ewrbrfc)

The resolver registry system has been implemented with the following structure:

1. **Domain Models** (`Domain/Model/`):

   - `Resolver.cs` - Resolver aggregate root with metadata
   - `ResolverVersion.cs` - Resolver version with Docker reference/tag

2. **Template-Resolver Link** (`App/Modules/Cyan/Data/Models/TemplateResolverData.cs`):

   - Junction table linking `TemplateVersionData` to `ResolverVersionData`
   - Currently only stores `Id`, `TemplateId`, `ResolverId`

3. **API Models** (`App/Modules/Cyan/API/V1/Models/TemplateVersionModel.cs`):

   - `TemplateVersionResp` includes `IEnumerable<ResolverVersionPrincipalResp> Resolvers`
   - `ResolverVersionPrincipalResp` only contains: `Id`, `Version`, `CreatedAt`, `Description`, `DockerReference`, `DockerTag`

4. **Current Response Structure**:
   ```csharp
   TemplateVersionResp(
     TemplateVersionPrincipalResp Principal,
     TemplatePrincipalResp Template,
     IEnumerable<PluginVersionPrincipalResp> Plugins,
     IEnumerable<ProcessorVersionPrincipalResp> Processors,
     IEnumerable<TemplateVersionPrincipalResp> Templates,
     IEnumerable<ResolverVersionPrincipalResp> Resolvers  // Missing config and files
   )
   ```

### Gap Analysis

The current `ResolverVersionPrincipalResp` and the template-resolver link table do NOT include:

1. **Resolver configuration** - The `config` object from cyan.yaml (arbitrary JSON)
2. **Glob patterns** - The `files` array from cyan.yaml (list of glob strings)

These are required by Iridium (CU-86ewrbrb0) to:

- Build a ResolverRegistry during VFS layering
- Match files against glob patterns to determine which resolver handles conflicts
- Pass the correct configuration to the resolver endpoint

---

## Required Changes

### 1. Database Schema Changes

**Modify `TemplateResolverVersionData`** (`App/Modules/Cyan/Data/Models/TemplateResolverData.cs`):

Add two new fields to store resolver configuration and glob patterns:

```csharp
public record TemplateResolverVersionData
{
  public Guid Id { get; set; }
  public Guid TemplateId { get; set; }
  public TemplateVersionData Template { get; set; } = null!;
  public Guid ResolverId { get; set; }
  public ResolverVersionData Resolver { get; set; } = null!;

  // NEW FIELDS - stored as JSON string in DB, converted to JsonElement in domain/API
  public string Config { get; set; } = "{}";  // JSON string in DB (mapped to JsonElement in domain)
  public string[] Files { get; set; } = Array.Empty<string>();  // Glob patterns
}
```

**Migration Required:**

- Add `config` column (JSONB or TEXT) to `TemplateResolverVersions` table
- Add `files` column (TEXT[]) to `TemplateResolverVersions` table

### 2. Domain Model Changes

The template-resolver relationship with its associated configuration and glob patterns requires proper modeling as an intermediate concept between `TemplateVersion` and `ResolverVersion`.

**New Domain Record: `TemplateVersionResolverRef`** (`Domain/Model/TemplateVersionResolverRef.cs`):

This record represents a template's reference to a specific resolver version, including the configuration and file patterns that apply to this specific pairing. The junction table Id is not needed in the domain model since the relationship is never queried independently:

```csharp
public record TemplateVersionResolverRef(
  ResolverVersionPrincipal Resolver,
  JsonElement Config,      // Dynamic JSON object
  string[] Files           // Glob patterns
);
```

**Modify `Domain/Model/TemplateVersion.cs`:**

Update `TemplateVersion` to reference the new aggregate:

```csharp
public record TemplateVersion
{
  public required TemplateVersionPrincipal Principal { get; init; }
  public required TemplatePrincipal TemplatePrincipal { get; init; }
  public required IEnumerable<PluginVersionPrincipal> Plugins { get; init; }
  public required IEnumerable<ProcessorVersionPrincipal> Processors { get; init; }
  public required IEnumerable<TemplateVersionPrincipal> Templates { get; init; }
  public required IEnumerable<TemplateVersionResolverRef> Resolvers { get; init; }  // Changed type
}
```

### 3. API Model Changes

**Modify `App/Modules/Cyan/API/V1/Models/TemplateVersionModel.cs`:**

Add a new response type that extends the resolver principal with config/files (flattened structure):

```csharp
/// <summary>
/// Resolver reference within a template version context.
/// Extends ResolverVersionPrincipalResp with config/files from the TemplateResolverVersions join table.
/// Flattened structure for backward compatibility.
/// </summary>
public record TemplateVersionResolverResp(
  // From ResolverVersionPrincipalResp (existing fields)
  Guid Id,
  uint Version,
  DateTime CreatedAt,
  string Description,
  string DockerReference,
  string DockerTag,
  // NEW: From join table
  JsonElement Config,  // Dynamic JSON object (stored as JSON string in DB)
  string[] Files       // Glob patterns
);
```

Update `TemplateVersionResp`:

```csharp
public record TemplateVersionResp(
  TemplateVersionPrincipalResp Principal,
  TemplatePrincipalResp Template,
  IEnumerable<PluginVersionPrincipalResp> Plugins,
  IEnumerable<ProcessorVersionPrincipalResp> Processors,
  IEnumerable<TemplateVersionPrincipalResp> Templates,
  IEnumerable<TemplateVersionResolverResp> Resolvers  // New type - was IEnumerable<ResolverVersionPrincipalResp>
);
```

**Update Request Models** (for CreateVersion/Push):

```csharp
public record ResolverReferenceReq(
  string Username,
  string Name,
  uint Version,
  JsonElement Config,  // NEW: From join table (dynamic JSON object, required)
  string[] Files       // NEW: From join table (glob patterns, required)
);
```

### 4. Service Layer Changes

**New Service Input Type** (`Domain/Service/TemplateVersionResolverInput.cs`):

```csharp
public record TemplateVersionResolverInput(
  ResolverVersionRef Resolver,
  JsonElement Config,  // Dynamic JSON object
  string[] Files
);
```

**Modify `Domain/Service/ITemplateService.cs`:**

Update method signatures to accept resolver inputs:

```csharp
Task<Result<TemplateVersionPrincipal?>> CreateVersion(
  string userId,
  string name,
  TemplateVersionRecord record,
  TemplateVersionProperty? property,
  IEnumerable<Guid> processors,
  IEnumerable<Guid> plugins,
  IEnumerable<Guid> templates,
  IEnumerable<TemplateVersionResolverInput> resolvers  // Changed
);
```

**Modify `Domain/Service/TemplateService.cs`:**

- Update `CreateVersion` and `Push` methods to handle resolver inputs
- Pass config and files to repository layer

### 5. Repository Layer Changes

**Modify `Domain/Repository/ITemplateRepository.cs`:**

Update method signatures:

```csharp
Task<Result<TemplateVersionPrincipal?>> CreateVersion(
  string userId,
  string name,
  TemplateVersionRecord record,
  TemplateVersionProperty? property,
  IEnumerable<Guid> processors,
  IEnumerable<Guid> plugins,
  IEnumerable<Guid> templates,
  IEnumerable<TemplateVersionResolverInput> resolvers
);
```

**Modify `App/Modules/Cyan/Data/Repositories/TemplateRepository.cs`:**

- Update `CreateVersion` methods to store `Config` (as JSON string) and `Files` in `TemplateResolverVersionData`
- Convert `JsonElement` to string using `JsonSerializer.Serialize()` before storage
- Update `GetVersion` methods to retrieve `Config` and `Files` from the junction table
- Update mapper calls to include the new data

### 6. Mapper Changes

**Modify `App/Modules/Cyan/Data/Mappers/TemplateMapper.cs`:**

Update `TemplateVersionMapper.ToDomain` to construct `TemplateVersionResolverRef`:

```csharp
public static TemplateVersion ToDomain(this TemplateVersionData data) =>
  new()
  {
    Principal = data.ToPrincipal(),
    TemplatePrincipal = data.Template.ToPrincipal(),
    Plugins = data.Plugins.Select(x => x.Plugin.ToPrincipal()).ToList(),
    Processors = data.Processors.Select(x => x.Processor.ToPrincipal()).ToList(),
    Templates = data.TemplateRefs.Select(x => x.TemplateRef.ToPrincipal()).ToList(),
    Resolvers = data.Resolvers.Select(x => new TemplateVersionResolverRef(
      x.Resolver.ToPrincipal(),
      JsonSerializer.Deserialize<JsonElement>(x.Config),  // Convert JSON string to JsonElement
      x.Files
    )).ToList(),
  };
```

**Modify `App/Modules/Cyan/API/V1/Mappers/TemplateMapper.cs`:**

Update `TemplateVersionMapper.ToResp` (flattened response):

```csharp
public static TemplateVersionResp ToResp(this TemplateVersion version) =>
  new(
    version.Principal.ToResp(),
    version.TemplatePrincipal.ToResp(),
    version.Plugins.Select(x => x.ToResp()),
    version.Processors.Select(x => x.ToResp()),
    version.Templates.Select(x => x.ToResp()),
    version.Resolvers.Select(x => new TemplateVersionResolverResp(
      // Flattened from ResolverVersionPrincipal
      x.Resolver.Id,
      x.Resolver.Version,
      x.Resolver.CreatedAt,
      x.Resolver.Description,
      x.Resolver.DockerReference,
      x.Resolver.DockerTag,
      // From join table (JsonElement passed through)
      x.Config,
      x.Files
    ))
  );
```

Update `ResolverReferenceReq.ToDomain` to return `TemplateVersionResolverInput`:

```csharp
public static TemplateVersionResolverInput ToDomain(this ResolverReferenceReq req) =>
  new(
    new ResolverVersionRef(req.Username, req.Name, req.Version == 0 ? null : req.Version),
    req.Config,  // JsonElement passed through
    req.Files
  );
```

### 7. Controller Changes

**Modify `App/Modules/Cyan/API/V1/Controllers/TemplateController.cs`:**

Update `CreateVersion` and `Push` methods to extract and pass resolver configs:

```csharp
var resolverRefs = (req.Resolvers ?? Array.Empty<ResolverReferenceReq>())
  .Select(r => r.ToDomain());

// Pass to service as tuples of (Ref, Config, Files)
```

---

## API Changes

### Modified Endpoints

| Method | Path                                                              | Change                                          |
| ------ | ----------------------------------------------------------------- | ----------------------------------------------- |
| GET    | `/api/v1/template/slug/{username}/{templateName}/versions/{ver}`  | Response now includes resolver config and files |
| GET    | `/api/v1/template/slug/{username}/{templateName}/versions/latest` | Response now includes resolver config and files |
| GET    | `/api/v1/template/id/{userId}/{templateId}/versions/{ver}`        | Response now includes resolver config and files |
| GET    | `/api/v1/template/versions/{versionId}`                           | Response now includes resolver config and files |
| POST   | `/api/v1/template/slug/{username}/{templateName}/versions`        | Accepts resolver config and files in request    |
| POST   | `/api/v1/template/id/{userId}/{templateId}/versions`              | Accepts resolver config and files in request    |
| POST   | `/api/v1/template/push/{username}`                                | Accepts resolver config and files in request    |

### Response Structure Comparison

**BEFORE (existing - ResolverVersionPrincipalResp):**

```json
{
  "resolvers": [
    {
      "id": "guid",
      "version": 1,
      "createdAt": "2024-...",
      "description": "JSON merger resolver",
      "dockerReference": "atomi/json-merger",
      "dockerTag": "1"
    }
  ]
}
```

**AFTER (new - TemplateVersionResolverResp - flattened, backward compatible, dynamic config):**

```json
{
  "resolvers": [
    {
      "id": "guid",
      "version": 1,
      "createdAt": "2024-...",
      "description": "JSON merger resolver",
      "dockerReference": "atomi/json-merger",
      "dockerTag": "1",
      "config": {
        "strategy": "deep-merge",
        "array_strategy": "append"
      },
      "files": ["package.json", "**/tsconfig.json"]
    }
  ]
}
```

### Request Structure Comparison (Template CreateVersion/Push Endpoints)

**BEFORE (existing request):**

```json
{
  "description": "New version",
  "properties": { ... },
  "plugins": [ ... ],
  "processors": [ ... ],
  "templates": [ ... ],
  "resolvers": [
    {
      "username": "atomi",
      "name": "json-merger",
      "version": 1
    }
  ]
}
```

**AFTER (new required fields with dynamic config):**

```json
{
  "description": "New version",
  "properties": { ... },
  "plugins": [ ... ],
  "processors": [ ... ],
  "templates": [ ... ],
  "resolvers": [
    {
      "username": "atomi",
      "name": "json-merger",
      "version": 1,
      "config": {},                          // NEW: required (dynamic JSON object)
      "files": ["package.json", "**/tsconfig.json"]  // NEW: required
    }
  ]
}
```

**Note:** `config` and `files` are now required fields in the request. `config` is a dynamic JSON object (not a string). Existing clients will need to be updated to include these fields.

---

## Files to Modify

### Domain Layer

1. `Domain/Model/TemplateVersionResolverRef.cs` - NEW: Domain record for template-resolver reference with config/files
2. `Domain/Model/TemplateVersion.cs` - Update `TemplateVersion.Resolvers` type to use new record

### Data Layer

2. `App/Modules/Cyan/Data/Models/TemplateResolverData.cs` - Add `Config` and `Files` fields
3. `App/Modules/Cyan/Data/Mappers/TemplateMapper.cs` - Update `ToDomain` mapper
4. `App/Modules/Cyan/Data/Repositories/TemplateRepository.cs` - Update all version methods

### Repository Interface

5. `Domain/Repository/ITemplateRepository.cs` - Update `CreateVersion` signatures

### Service Layer

6. `Domain/Service/TemplateVersionResolverInput.cs` - NEW: Proper type for resolver input
7. `Domain/Service/ITemplateService.cs` - Update `CreateVersion` and `Push` signatures
8. `Domain/Service/TemplateService.cs` - Update implementation

### API Layer

9. `App/Modules/Cyan/API/V1/Models/TemplateVersionModel.cs` - Add `TemplateVersionResolverResp`, update `ResolverReferenceReq`, update `TemplateVersionResp`
10. `App/Modules/Cyan/API/V1/Mappers/TemplateMapper.cs` - Update `ToResp` and `ToDomain` mappers
11. `App/Modules/Cyan/API/V1/Controllers/TemplateController.cs` - Update controller methods

### Database

12. New migration to add `config` and `files` columns to `TemplateResolverVersions` table

---

## Acceptance Criteria

### Data Model Changes

- [ ] `TemplateResolverVersionData` includes `Config` (string, JSON) and `Files` (string array) fields
- [ ] `TemplateVersionResolverRef` domain record exists with `Resolver`, `Config` (JsonElement), and `Files` properties
- [ ] `TemplateVersion.Resolvers` returns `IEnumerable<TemplateVersionResolverRef>`

### API Response Changes

- [ ] `TemplateVersionResolverResp` NEW API model exists with flattened resolver fields (`Id`, `Version`, `Description`, `DockerReference`, `DockerTag`) plus `Config` (JsonElement) and `Files`
- [ ] `ResolverVersionPrincipalResp` remains unchanged (existing API preserved)
- [ ] `TemplateVersionResp.Resolvers` returns `IEnumerable<TemplateVersionResolverResp>` (changed from `IEnumerable<ResolverVersionPrincipalResp>`)
- [ ] GET `/api/v1/template/.../versions/...` endpoints include resolver config and files in response

### API Request Changes

- [ ] `ResolverReferenceReq` includes required `Config` (JsonElement) and `Files` fields
- [ ] POST endpoints accept resolver config and files
- [ ] `TemplateVersionResolverInput` service type exists with `Resolver`, `Config`, `Files`
- [ ] Controller maps requests to `TemplateVersionResolverInput`

### Database Changes

- [ ] Migration adds `config` column (TEXT/JSONB) to `TemplateResolverVersions`
- [ ] Migration adds `files` column (TEXT[]) to `TemplateResolverVersions`
- [ ] Existing records have default values (empty config, empty files array)

### Backward Compatibility

- [ ] `ResolverVersionPrincipalResp` unchanged - existing resolver endpoints unaffected
- [ ] Response is additive: existing fields remain, new `config` and `files` fields added at end
- [ ] Old clients reading responses will ignore new fields (backward compatible)
- [ ] Existing data without configs/files returns empty defaults (migration handles this)

### Integration

- [ ] Iridium can read resolver configs from `TemplateVersionResp`
- [ ] Iridium can read glob patterns from `TemplateVersionResp`
- [ ] Data flows correctly from cyan.yaml through push to API response

---

## Definition of Done

- [ ] All acceptance criteria met
- [ ] Database migration created and tested
- [ ] Code compiles without errors (`pls build`)
- [ ] `pre-commit run --all` passes
- [ ] Ticket ID included in commit message
- [ ] API tested on http://localhost:9001
- [ ] Backward compatibility maintained
- [ ] Response structure matches specification

---

## Technical Decisions

| Decision              | Choice                                                          | Reasoning                                                                                                         |
| --------------------- | --------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| Config storage format | JSON string in TEXT column                                      | Flexible schema for resolver-specific configs; PostgreSQL JSONB could be used for indexing if needed later        |
| Files storage format  | TEXT[] array                                                    | Native PostgreSQL array type; efficient storage for glob patterns                                                 |
| Domain modeling       | `TemplateVersionResolverRef` as simple record                   | Cleanly encapsulates the template-resolver relationship with config/files; junction table Id not needed in domain |
| API response format   | Flattened structure (extends existing fields with config/files) | Backward compatible - old clients ignore new fields; new clients get full data                                    |
| Service input type    | `TemplateVersionResolverInput` proper type                      | Cleaner than tuple; clear field names; type safety                                                                |
| API compatibility     | New response type `TemplateVersionResolverResp`                 | Existing `ResolverVersionPrincipalResp` unchanged; no breaking changes to resolver endpoints                      |
| Config API type       | `JsonElement` (dynamic JSON)                                    | Clean JSON object in API instead of escaped JSON string; serialized to string for DB storage                      |
| Files default value   | `[]` (empty array)                                              | Clear indication that no files are matched; set by client when no files specified                                 |
| Request config/files  | Required fields                                                 | Client must explicitly provide values (can be empty)                                                              |

---

## Dependencies

**Blocked by:**

- CU-86ewrbrfc - Resolver registry system must be implemented first

**Blocks:**

- CU-86ewrbrb0 - Iridium layerer resolver integration

---

## Commands

```bash
# Build
pls build

# Create migration
pls migration:create -- AddResolverConfigToTemplateResolverVersions

# Run dev server (http://localhost:9001)
pls dev

# Pre-commit check
pre-commit run --all
```

---

## Reference Files

| What                          | Reference File                                             |
| ----------------------------- | ---------------------------------------------------------- |
| Current TemplateVersion model | `Domain/Model/TemplateVersion.cs`                          |
| Current TemplateResolver data | `App/Modules/Cyan/Data/Models/TemplateResolverData.cs`     |
| Current API models            | `App/Modules/Cyan/API/V1/Models/TemplateVersionModel.cs`   |
| Current mapper                | `App/Modules/Cyan/API/V1/Mappers/TemplateMapper.cs`        |
| Current repository            | `App/Modules/Cyan/Data/Repositories/TemplateRepository.cs` |
| Resolver registry spec        | `spec/CU-86ewrbrfc/v1/task-spec.md`                        |
| Parent resolver system        | `spec/CU-86ewuf8zx/ticket.md`                              |
