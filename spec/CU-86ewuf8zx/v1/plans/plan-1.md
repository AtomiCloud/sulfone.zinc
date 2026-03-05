# Plan 1: Domain, Data, Service, and Repository Layer Changes

## Goal

Add `Config` (JSON) and `Files` (glob patterns) to the template-resolver junction throughout all internal layers: domain model, data model, service, repository, data mappers, and database migration.

## Scope

- Create `TemplateVersionResolverRef` domain record (NEW)
- Create `TemplateVersionResolverInput` service input type (NEW)
- Update `TemplateVersion` domain model
- Update `TemplateResolverVersionData` data model
- Update `ITemplateRepository` and `TemplateRepository`
- Update `ITemplateService` and `TemplateService`
- Update data mapper (`App/Modules/Cyan/Data/Mappers/TemplateMapper.cs`)
- Create EF Core migration

## Files to Create

### 1. `Domain/Model/TemplateVersionResolverRef.cs` (NEW)

Domain record representing a template's reference to a resolver version with associated config and file patterns.

```csharp
public record TemplateVersionResolverRef(
  ResolverVersionPrincipal Resolver,
  JsonElement Config,
  string[] Files
);
```

- `Resolver` — the resolved resolver version principal (has `Id`, `Version` (ulong), `CreatedAt`, `Record`, `Property`)
- `Config` — dynamic JSON object (`JsonElement`, not string)
- `Files` — glob patterns (e.g., `["package.json", "**/tsconfig.json"]`)

### 2. `Domain/Service/TemplateVersionResolverInput.cs` (NEW)

Service-layer input type for passing resolver data from API to service.

```csharp
public record TemplateVersionResolverInput(
  ResolverVersionRef Resolver,
  JsonElement Config,
  string[] Files
);
```

- `Resolver` — unresolved reference (`Username`, `Name`, `Version?`) used for lookup
- The service resolves this to a `ResolverVersionPrincipal` (with Guid `Id`) via `IResolverRepository.GetAllVersion()`

## Files to Modify

### 3. `Domain/Model/TemplateVersion.cs`

Change `Resolvers` property type:

| Property    | FROM                                    | TO                                        |
| ----------- | --------------------------------------- | ----------------------------------------- |
| `Resolvers` | `IEnumerable<ResolverVersionPrincipal>` | `IEnumerable<TemplateVersionResolverRef>` |

### 4. `App/Modules/Cyan/Data/Models/TemplateResolverData.cs`

Add two fields to `TemplateResolverVersionData`:

| New Property | Type       | Default                 | DB Column          |
| ------------ | ---------- | ----------------------- | ------------------ |
| `Config`     | `string`   | `"{}"`                  | TEXT (JSON string) |
| `Files`      | `string[]` | `Array.Empty<string>()` | TEXT[]             |

### 5. `Domain/Repository/ITemplateRepository.cs`

Both `CreateVersion` overloads: change `resolvers` parameter from `IEnumerable<Guid>` to `IEnumerable<TemplateVersionResolverInput>`.

**Overload 1 (by username):**

```csharp
Task<Result<TemplateVersionPrincipal?>> CreateVersion(
  string username, string name,
  TemplateVersionRecord record, TemplateVersionProperty? property,
  IEnumerable<Guid> processors, IEnumerable<Guid> plugins,
  IEnumerable<Guid> templates,
  IEnumerable<TemplateVersionResolverInput> resolvers  // ← CHANGED from IEnumerable<Guid>
);
```

**Overload 2 (by userId):** Same change to the `resolvers` parameter.

> **Design note:** We pass `TemplateVersionResolverInput` (which has `ResolverVersionRef`) not just `Guid` because the repo needs Config and Files. The repo must extract the resolver ID differently — see repo implementation below.

**WAIT — the current pattern passes resolved Guid IDs to the repo.** The service currently calls `resolver.GetAllVersion(resolvers)` to resolve refs → principals → extract IDs. With the new approach, the service must:

1. Extract `ResolverVersionRef` from each input
2. Call `resolver.GetAllVersion(refs)` to resolve → get principals with GUIDs
3. Match resolved principals back to inputs (by username/name) to re-associate Config/Files
4. Pass matched data to repo

So the repo should actually accept a resolved type. Create a simple record in the repo interface:

```csharp
// Could be a nested type or in the same file as the interface
public record ResolverLink(Guid ResolverId, JsonElement Config, string[] Files);
```

And change the repo parameter to `IEnumerable<ResolverLink>`.

### 6. `Domain/Service/ITemplateService.cs`

Update `CreateVersion` (both overloads) and `Push` method signatures:

| Method               | Parameter   | FROM                              | TO                                          |
| -------------------- | ----------- | --------------------------------- | ------------------------------------------- |
| `CreateVersion` (×2) | `resolvers` | `IEnumerable<ResolverVersionRef>` | `IEnumerable<TemplateVersionResolverInput>` |
| `Push`               | `resolvers` | `IEnumerable<ResolverVersionRef>` | `IEnumerable<TemplateVersionResolverInput>` |

### 7. `Domain/Service/TemplateService.cs`

**Key change in `CreateVersion`:** The current pattern is:

```csharp
var resolverResults = await resolver.GetAllVersion(resolvers); // resolvers = IEnumerable<ResolverVersionRef>
// ... LINQ query ...
resolver.Select(x => x.Id)  // extracts Guid IDs only
// passes IEnumerable<Guid> to repo.CreateVersion()
```

**New pattern:**

```csharp
// 1. Extract refs for resolution
var resolverRefs = resolvers.Select(r => r.Resolver);

// 2. Resolve refs to principals (existing call)
var resolverResults = await resolver.GetAllVersion(resolverRefs);

// 3. In the LINQ chain, match resolved principals back to inputs
// The matching key is (Username, Name) from the ref
// Create ResolverLink records with resolved Guid + Config + Files
var resolverLinks = resolverResults.Select(principals =>
  principals.Select(p => {
    var input = resolverInputs.First(i =>
      // Match by the resolver identity
      // The exact matching depends on what GetAllVersion returns and how refs correlate
    );
    return new ResolverLink(p.Id, input.Config, input.Files);
  })
);

// 4. Pass IEnumerable<ResolverLink> to repo.CreateVersion()
```

**Important:** Examine `IResolverRepository.GetAllVersion()` to understand the return type and how to correlate resolved results back to inputs. The matching strategy depends on whether results come back in the same order as inputs.

Apply the same pattern to the `Push` method.

### 8. `App/Modules/Cyan/Data/Repositories/TemplateRepository.cs`

Both `CreateVersion` methods: update resolver link creation.

**Current:**

```csharp
var resolverLinks = resolvers.Select(x => new TemplateResolverVersionData
{
  ResolverId = x,
  Resolver = null!,
  TemplateId = t.Id,
  Template = null!,
});
```

**New:**

```csharp
var resolverLinks = resolvers.Select(x => new TemplateResolverVersionData
{
  ResolverId = x.ResolverId,
  Resolver = null!,
  TemplateId = t.Id,
  Template = null!,
  Config = JsonSerializer.Serialize(x.Config),
  Files = x.Files,
});
```

### 9. `App/Modules/Cyan/Data/Mappers/TemplateMapper.cs`

Update `TemplateVersionMapper.ToDomain` to construct `TemplateVersionResolverRef`:

**Current:**

```csharp
Resolvers = data.Resolvers.Select(x => x.Resolver.ToPrincipal()).ToList(),
```

**New:**

```csharp
Resolvers = data.Resolvers.Select(x => new TemplateVersionResolverRef(
  x.Resolver.ToPrincipal(),
  JsonSerializer.Deserialize<JsonElement>(x.Config),
  x.Files
)).ToList(),
```

### 10. Database Migration

Create migration: `AddResolverConfigAndFilesToTemplateResolverVersions`

```bash
direnv exec . pls migration:create -- AddResolverConfigAndFilesToTemplateResolverVersions
```

Expected migration operations:

- Add `Config` column (TEXT, default `'{}'`, NOT NULL) to `TemplateResolverVersions`
- Add `Files` column (TEXT[], default `ARRAY[]::text[]`, NOT NULL) to `TemplateResolverVersions`

## Key Design Decisions

| Decision            | Choice                                      | Reasoning                                                 |
| ------------------- | ------------------------------------------- | --------------------------------------------------------- |
| Repo parameter type | `ResolverLink(Guid, JsonElement, string[])` | Clean separation — repo works with resolved IDs, not refs |
| Config DB type      | TEXT (not JSONB)                            | No need for JSON indexing; stored as opaque blob          |
| Files DB type       | TEXT[]                                      | Native PostgreSQL array; simple and efficient             |
| Service matching    | Correlate resolved results with inputs      | Must preserve Config/Files association through resolution |

## Edge Cases

| Case                            | Handling                                                                    |
| ------------------------------- | --------------------------------------------------------------------------- |
| Invalid JSON in Config from DB  | `JsonSerializer.Deserialize<JsonElement>` throws — wrap or let it fail fast |
| Empty Config `"{}"`             | Valid — empty object, passes through                                        |
| Empty Files `[]`                | Valid — no file matching rules                                              |
| Null Config in DB (legacy rows) | Migration sets default `'{}'` — no nulls                                    |

## Implementation Checklist

- [ ] `TemplateVersionResolverRef` domain record created
- [ ] `TemplateVersionResolverInput` service input type created
- [ ] `TemplateVersion.Resolvers` type changed to `IEnumerable<TemplateVersionResolverRef>`
- [ ] `TemplateResolverVersionData` has `Config` and `Files` fields
- [ ] `ITemplateRepository.CreateVersion` both overloads updated
- [ ] `TemplateRepository.CreateVersion` stores Config/Files
- [ ] `ITemplateService.CreateVersion` and `Push` updated
- [ ] `TemplateService` implementation handles resolver input resolution + re-association
- [ ] Data mapper constructs `TemplateVersionResolverRef` with deserialized Config
- [ ] Migration created and applies cleanly
- [ ] `direnv exec . pls build` compiles (may fail on API layer — that's Plan 2)

## Documentation Requirements

- [ ] Explore `docs/developer/` directory structure to understand existing documentation patterns
- [ ] Document the new `Config` and `Files` fields on the template-resolver junction: what they are, how they flow from cyan.yaml through the API, storage format decisions (JSON string in TEXT column, TEXT[] for files)
- [ ] Document the `TemplateVersionResolverRef` domain record and `TemplateVersionResolverInput` service type — their purpose and how they relate
- [ ] Document the `ResolverLink` repo parameter type and why it differs from the service input type
- [ ] Write documentation in the same format and location as existing docs (follow the style, structure, and tone of existing files)
- [ ] Add inline code comments for the service-layer matching logic (correlating resolved principals back to inputs)

## Non-Functional Checklist

- [ ] `direnv exec . pre-commit run --all` passes
- [ ] No new compiler warnings
- [ ] Code follows existing patterns (record types, naming conventions)
- [ ] Performance: no N+1 queries introduced
