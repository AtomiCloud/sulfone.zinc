# Resolver Registry Feature

**What**: CRUD operations for resolvers with versioning and template associations.
**Why**: Stores resolver component definitions that can be linked to template versions with custom configuration.

**Key Files**:

- `Domain/Service/ResolverService.cs` → `Create()`, `Update()`, `Delete()`
- `App/Modules/Cyan/Data/Repositories/ResolverRepository.cs` → Data access
- `App/Modules/Cyan/API/V1/Controllers/ResolverController.cs` → Endpoints

## Overview

The Resolver Registry manages resolver components. Like templates, resolvers contain metadata and versions. Resolvers can be referenced by template versions with associated configuration and file patterns.

For the conceptual overview of registry structure, see [Registry Concept](../concepts/03-registry.md). For version management, see [Version Concept](../concepts/04-version.md).

## Operations

| Operation   | Endpoint                                      | Key File                        |
| ----------- | --------------------------------------------- | ------------------------------- |
| Search      | `GET /api/v1/resolver`                        | `ResolverController.cs:37-48`   |
| Get by ID   | `GET /api/v1/resolver/id/{userId}/{id:guid}`  | `ResolverController.cs:50-58`   |
| Get by slug | `GET /api/v1/resolver/slug/{username}/{name}` | `ResolverController.cs:60-68`   |
| Create      | `POST /api/v1/resolver/id/{userId}`           | `ResolverController.cs:70-90`   |
| Update      | `PUT /api/v1/resolver/id/{userId}/{id}`       | `ResolverController.cs:92-116`  |
| Delete      | `DELETE /api/v1/resolver/id/{userId}/{id}`    | `ResolverController.cs:148-156` |

## Version Operations

| Operation      | Endpoint                                                      | Key File                        |
| -------------- | ------------------------------------------------------------- | ------------------------------- |
| Get version    | `GET /api/v1/resolver/slug/{username}/{name}/versions/{ver}`  | `ResolverController.cs:186-205` |
| Get latest     | `GET /api/v1/resolver/slug/{username}/{name}/versions/latest` | `ResolverController.cs:207-225` |
| Create version | `POST /api/v1/resolver/slug/{username}/{name}/versions`       | `ResolverController.cs:247-282` |
| Push           | `POST /api/v1/resolver/push/{username}`                       | `ResolverController.cs:341-375` |

## Template-Resolver Association

Template versions can reference resolvers with additional metadata:

### Config and Files Fields

| Field  | Type          | Purpose                                     | Storage Format                   |
| ------ | ------------- | ------------------------------------------- | -------------------------------- |
| Config | `JsonElement` | Dynamic JSON configuration for the resolver | TEXT column (JSON string)        |
| Files  | `string[]`    | Glob patterns for file matching             | TEXT[] column (PostgreSQL array) |

### Type Flow Through Layers

```text
API Layer                Service Layer             Repository Layer          Domain Model
-----------              ----------------          -----------------         -------------
ResolverReferenceReq → TemplateVersionResolverInput → ResolverLink → TemplateVersionResolverRef
   (ResolverVersionRef,     (ResolverId,        (ResolverVersionPrincipal,
    Config, Files)           Config, Files)       Config, Files)
```

### Type Reference

#### TemplateVersionResolverInput (Service Layer Input)

Service-layer input type for passing resolver data from API to service.

**File**: `Domain/Service/TemplateVersionResolverInput.cs`

```csharp
public record TemplateVersionResolverInput(
  ResolverVersionRef Resolver,  // Unresolved reference (Username, Name, Version?)
  JsonElement Config,           // Dynamic JSON configuration
  string[] Files                // Glob patterns (e.g., ["package.json", "**/tsconfig.json"])
);
```

#### ResolverLink (Repository Layer)

Repository-layer type for storing resolver associations. Uses resolved GUID instead of ref.

**File**: `Domain/Repository/ITemplateRepository.cs`

```csharp
public record ResolverLink(
  Guid ResolverId,    // Resolved GUID from ResolverVersionPrincipal.Id
  JsonElement Config, // Preserved from input
  string[] Files      // Preserved from input
);
```

#### TemplateVersionResolverRef (Domain Model)

Domain model for reading template-resolver relationships. Contains fully resolved principal.

**File**: `Domain/Model/TemplateVersionResolverRef.cs`

```csharp
public record TemplateVersionResolverRef(
  ResolverVersionPrincipal Resolver, // Fully resolved version principal
  JsonElement Config,                // From storage
  string[] Files                     // From storage
);
```

#### ResolverVersionWithIdentity (Resolution Helper)

Wraps resolver version principal with identity info for correlation.

**File**: `Domain/Repository/IResolverRepository.cs`

```csharp
public record ResolverVersionWithIdentity(
  string Username,              // For correlation back to input
  string Name,                  // For correlation back to input
  ResolverVersionPrincipal Principal
);
```

### Resolution and Matching

The `CreateResolverLinks` helper in `TemplateService` correlates resolved resolvers back to their original inputs to preserve Config and Files:

**File**: `Domain/Service/TemplateService.cs:322-342`

```csharp
private static ResolverLink[] CreateResolverLinks(
  TemplateVersionResolverInput[] inputs,
  IEnumerable<ResolverVersionWithIdentity> resolvedResolvers
)
{
  // Create a lookup from (Username, Name) to resolved principal
  var resolvedLookup = resolvedResolvers.ToDictionary(
    r => (r.Username, r.Name),
    r => r.Principal
  );

  // For each input, find the corresponding resolved principal and create a ResolverLink
  return inputs
    .Select(input =>
    {
      var key = (input.Resolver.Username, input.Resolver.Name);
      var principal = resolvedLookup[key];
      return new ResolverLink(principal.Id, input.Config, input.Files);
    })
    .ToArray();
}
```

### Storage Decisions

| Decision              | Choice         | Rationale                                                                                 |
| --------------------- | -------------- | ----------------------------------------------------------------------------------------- |
| Config storage        | TEXT column    | No need for JSON indexing at DB level; Config is opaque blob from DB perspective          |
| Files storage         | TEXT[] array   | Native PostgreSQL array support; efficient storage and querying; EF Core built-in support |
| ResolverLink vs Input | Separate types | Clean separation - repo works with resolved GUIDs, service handles resolution logic       |

## Edge Cases

| Case                         | Behavior         |
| ---------------------------- | ---------------- |
| Duplicate name (same user)   | 409 Conflict     |
| Update non-existent resolver | 404 Not Found    |
| Delete non-existent resolver | 404 Not Found    |
| User mismatch                | 401 Unauthorized |

## Like System

Resolvers support user likes:

| Operation | Endpoint                                                            | Purpose           |
| --------- | ------------------------------------------------------------------- | ----------------- |
| Like      | `POST /api/v1/resolver/slug/{username}/{name}/like/{likerId}/true`  | Like a resolver   |
| Unlike    | `POST /api/v1/resolver/slug/{username}/{name}/like/{likerId}/false` | Unlike a resolver |

## Related

- [Registry Concept](../concepts/03-registry.md) - Registry entity structure
- [Version Concept](../concepts/04-version.md) - Version management
- [Template Registry Feature](./03-template-registry.md) - How templates reference resolvers
- [Cyan Module](../modules/02-cyan.md) - Code organization
