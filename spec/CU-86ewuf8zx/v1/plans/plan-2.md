# Plan 2: API Layer Changes (Models, Mappers, Validators, Controller)

## Goal

Expose resolver Config and Files through the API surface: update response models, request models, mappers, validators, and verify controller integration.

## Prerequisites

Plan 1 must be complete — this plan depends on:

- `TemplateVersionResolverRef` (domain record)
- `TemplateVersionResolverInput` (service input type)
- Updated service/repo signatures

## Files to Modify

### 1. `App/Modules/Cyan/API/V1/Models/TemplateVersionModel.cs`

**Add new response record — `TemplateVersionResolverResp`:**

Flattened structure combining resolver version principal fields with config/files from the join table.

```csharp
public record TemplateVersionResolverResp(
  Guid Id,
  ulong Version,                // NOTE: ulong, not uint — matches ResolverVersionPrincipal
  DateTime CreatedAt,
  string Description,
  string DockerReference,
  string DockerTag,
  JsonElement Config,           // NEW: from join table
  string[] Files                // NEW: from join table
);
```

**Update `TemplateVersionResp`:**

| Property    | FROM                                        | TO                                         |
| ----------- | ------------------------------------------- | ------------------------------------------ |
| `Resolvers` | `IEnumerable<ResolverVersionPrincipalResp>` | `IEnumerable<TemplateVersionResolverResp>` |

**Update `ResolverReferenceReq`:**

```csharp
// BEFORE:
public record ResolverReferenceReq(string Username, string Name, uint Version);

// AFTER:
public record ResolverReferenceReq(
  string Username,
  string Name,
  uint Version,
  JsonElement Config,           // NEW: required
  string[] Files                // NEW: required
);
```

**Note:** `ResolverVersionPrincipalResp` remains unchanged — it's still used by resolver-specific endpoints (`ResolverVersionModel.cs`).

### 2. `App/Modules/Cyan/API/V1/Mappers/TemplateMapper.cs`

**Create custom mapper for `TemplateVersionResolverRef` → `TemplateVersionResolverResp`:**

Do NOT reuse the resolver module's `ToResp()` extension method from `ResolverMapper.cs`. Create a dedicated, independent mapper within the template mapper file. This keeps the template and resolver API concerns fully decoupled — changes to the resolver module's mappers should never affect the template API and vice versa.

```csharp
// NEW — custom mapper, independent from ResolverMapper
public static TemplateVersionResolverResp ToTemplateResolverResp(this TemplateVersionResolverRef resolverRef) =>
  new(
    resolverRef.Resolver.Id,
    resolverRef.Resolver.Version,
    resolverRef.Resolver.CreatedAt,
    resolverRef.Resolver.Record.Description,
    resolverRef.Resolver.Property.DockerReference,
    resolverRef.Resolver.Property.DockerTag,
    resolverRef.Config,
    resolverRef.Files
  );
```

**Update `TemplateVersionMapper.ToResp` to use the custom mapper:**

```csharp
// Current:
version.Resolvers.Select(x => x.ToResp())

// New:
version.Resolvers.Select(x => x.ToTemplateResolverResp())
```

**Important:** `ResolverVersionPrincipal` has nested properties:

- `Record.Description` (not a flat `Description`)
- `Property.DockerReference` (not a flat `DockerReference`)
- `Property.DockerTag` (not a flat `DockerTag`)

Verify the exact property paths by reading `ResolverVersionPrincipal` and compare with (but do not depend on) the existing `ToResp()` extension method in `App/Modules/Cyan/API/V1/Mappers/ResolverMapper.cs`.

**Update request mapper — `ResolverReferenceReq.ToDomain`:**

```csharp
// BEFORE:
public static ResolverVersionRef ToDomain(this ResolverReferenceReq req) =>
  new(req.Username, req.Name, req.Version == 0 ? null : req.Version);

// AFTER:
public static TemplateVersionResolverInput ToDomain(this ResolverReferenceReq req) =>
  new(
    new ResolverVersionRef(req.Username, req.Name, req.Version == 0 ? null : req.Version),
    req.Config,
    req.Files
  );
```

### 3. `App/Modules/Cyan/API/V1/Validators/TemplateVersionValidator.cs`

**Update `ResolverReferenceReqValidator`:**

```csharp
// Current:
public class ResolverReferenceReqValidator : AbstractValidator<ResolverReferenceReq>
{
  public ResolverReferenceReqValidator()
  {
    this.RuleFor(x => x.Username).NotNull().UsernameValid();
    this.RuleFor(x => x.Name).NotNull().UsernameValid();
  }
}

// Add:
this.RuleFor(x => x.Files).NotNull();
```

**Note on Config validation:** `JsonElement` is a value type — it cannot be null in the C# sense. ASP.NET model binding will deserialize `null` JSON to `default(JsonElement)` (which has `ValueKind == Undefined`). Decide whether to validate against `Undefined` or accept any JsonElement. Follow the pattern used elsewhere in the codebase for JsonElement validation.

### 4. `App/Modules/Cyan/API/V1/Controllers/TemplateController.cs`

**No code changes required.** The controller already calls:

```csharp
(c.Resolvers ?? Array.Empty<ResolverReferenceReq>()).Select(r => r.ToDomain())
```

Since `ToDomain()` now returns `TemplateVersionResolverInput` instead of `ResolverVersionRef`, and the service now accepts `IEnumerable<TemplateVersionResolverInput>`, the types flow correctly. Verify compilation only.

## API Contract Summary

### Response (GET endpoints) — Additive, backward compatible

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
      "config": { "strategy": "deep-merge" },
      "files": ["package.json", "**/tsconfig.json"]
    }
  ]
}
```

### Request (POST endpoints) — Breaking change: new required fields

```json
{
  "resolvers": [
    {
      "username": "atomi",
      "name": "json-merger",
      "version": 1,
      "config": { "strategy": "deep-merge" },
      "files": ["package.json"]
    }
  ]
}
```

Existing clients that POST template versions with resolvers will need to add `config` and `files` fields.

## Edge Cases

| Case                                          | Handling                                                                                                           |
| --------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| Old client sends request without config/files | ASP.NET model binding: `Config` defaults to `default(JsonElement)`, `Files` is null → validator catches null Files |
| Empty config `{}`                             | Valid                                                                                                              |
| Empty files `[]`                              | Valid                                                                                                              |
| Config with deeply nested objects             | Passes through as `JsonElement` — no parsing needed                                                                |

## Documentation Requirements

- [ ] Explore `docs/developer/` directory structure to understand existing documentation patterns
- [ ] Document the new API response structure: `TemplateVersionResolverResp` fields, the flattened design, and backward compatibility guarantees
- [ ] Document the updated request structure: new required `config` and `files` fields on `ResolverReferenceReq`, and the breaking change for existing clients
- [ ] Document the custom mapper design decision: `ToTemplateResolverResp()` is independent from `ResolverMapper.ToResp()` to keep template and resolver API concerns decoupled
- [ ] Document the full data flow: `cyan.yaml` → `ResolverReferenceReq` (API) → `TemplateVersionResolverInput` (service) → `ResolverLink` (repo) → `TemplateResolverVersionData` (DB) → `TemplateVersionResolverRef` (domain) → `TemplateVersionResolverResp` (API response)
- [ ] Write documentation in the same format and location as existing docs (follow the style, structure, and tone of existing files)

## Implementation Checklist

- [ ] `TemplateVersionResolverResp` record added to `TemplateVersionModel.cs`
- [ ] `TemplateVersionResp.Resolvers` type changed to `IEnumerable<TemplateVersionResolverResp>`
- [ ] `ResolverReferenceReq` has `Config` and `Files` fields
- [ ] Custom `ToTemplateResolverResp()` mapper created (independent from ResolverMapper)
- [ ] Response mapper uses custom mapper to flatten `TemplateVersionResolverRef` → `TemplateVersionResolverResp`
- [ ] Request mapper returns `TemplateVersionResolverInput` instead of `ResolverVersionRef`
- [ ] Validator updated for new fields
- [ ] Controller compiles without changes
- [ ] `ResolverVersionPrincipalResp` unchanged (no breaking changes to resolver endpoints)
- [ ] `direnv exec . pls build` compiles successfully
- [ ] `direnv exec . pre-commit run --all` passes
- [ ] API response matches expected JSON structure
- [ ] Documentation written and reviewed
