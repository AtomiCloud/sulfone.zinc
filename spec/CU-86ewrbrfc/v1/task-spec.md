# Task Specification: [Zn] Allow users to create resolvers (CU-86ewrbrfc)

## Summary

Implement a Resolver Registry system for Zinc following the existing Processor/Plugin patterns. This includes domain models, repository, service, API endpoints, database schema, template-resolver dependencies, and documentation.

**Ticket:** CU-86ewrbrfc | **System:** ClickUp | **URL:** https://app.clickup.com/t/86ewrbrfc

---

## Acceptance Criteria

### Core Resolver Implementation

- [ ] Domain models: `Resolver.cs`, `ResolverVersion.cs` in `Domain/Model/`
- [ ] Repository interface: `IResolverRepository.cs` in `Domain/Repository/`
- [ ] Repository implementation in `App/Modules/Cyan/Data/Repositories/`
- [ ] Service layer: `IResolverService.cs`, `ResolverService.cs` in `Domain/Service/`
- [ ] Data models: `ResolverData.cs`, `ResolverVersionData.cs`, `ResolverLikeData.cs`
- [ ] API controller: `ResolverController.cs` with full CRUD + versioning
- [ ] API models: Request/Response DTOs, validators, mappers
- [ ] Database migration for new tables

### Template-Resolver Dependencies

- [ ] Junction table `TemplateResolverVersionData.cs` created
- [ ] `TemplateVersion` record includes `Resolvers` collection
- [ ] `TemplateVersionData` includes `Resolvers` navigation
- [ ] `TemplateRepository` handles resolver version lookups
- [ ] `TemplateService` validates resolver refs in CreateVersion/Push
- [ ] Backward compatibility: old templates return empty `Resolvers` array

### Integration & Documentation

- [ ] Services registered in DI container
- [ ] MainDbContext updated with DbSets and entity configurations
- [ ] Feature documentation: `docs/developer/features/06-resolver-registry.md`
- [ ] API documentation: `docs/developer/surfaces/api/06-resolver.md`
- [ ] Dependency documentation: `docs/developer/concepts/05-dependency.md` updated
- [ ] README updated with Resolver Registry in feature list

---

## Definition of Done

- [ ] All acceptance criteria met
- [ ] Code compiles without errors (`pls build`)
- [ ] `pre-commit run --all` passes
- [ ] Ticket ID included in commit message
- [ ] API tested on http://localhost:9001
- [ ] Structure matches processors/plugins exactly:
  - Domain models follow same record patterns
  - Repository interface follows same method signatures
  - Service follows same patterns (Create, Update, Delete, Like, Push)
  - API routes use `id/` and `slug/` prefixes
  - Data models include same fields and relationships
- [ ] Update operations only modify metadata (description), NOT references
- [ ] Template-resolver dependency integration complete
- [ ] Backward compatibility maintained for existing templates

---

## Technical Decisions

| Decision          | Choice                           | Reasoning                                     |
| ----------------- | -------------------------------- | --------------------------------------------- |
| API Route Style   | `id/` and `slug/` prefixes       | Consistency with processor/plugin patterns    |
| Version Format    | Numeric (`ulong`)                | Matches processor/plugin versioning           |
| Like/Star Feature | Included                         | Part of the full registry pattern             |
| Authorization     | Follow processor pattern exactly | GET=public, C/U/Push=owner-only, Delete=admin |
| Update Scope      | Metadata only (description)      | References set at Create/Push only            |
| Tests             | Skipped                          | No existing test patterns                     |

---

## Authorization Pattern

Follow the Processor/Template controllers exactly:

| Category       | Endpoints                                          | Attribute                                      | Code Check                                    | Who Can Perform                                  |
| -------------- | -------------------------------------------------- | ---------------------------------------------- | --------------------------------------------- | ------------------------------------------------ |
| **Public**     | All GET endpoints                                  | None                                           | None                                          | Anyone                                           |
| **Owner only** | Create, Update, CreateVersion, UpdateVersion, Push | `[Authorize]`                                  | `sub == userId` or `username's userId == sub` | Authenticated user modifying their OWN resources |
| **Liker only** | Like                                               | `[Authorize]`                                  | `sub == likerId`                              | Authenticated user liking for themselves         |
| **Admin only** | Delete                                             | `[Authorize(Policy = AuthPolicies.OnlyAdmin)]` | None                                          | Admin role required                              |

**Important:** Update operations only modify metadata (description). References like DockerReference, DockerTag, and dependencies are immutable after creation.

---

## API Endpoints

**Base Path:** `/api/v1/resolver`

| Method | Path                                            | Description              | Who Can Perform |
| ------ | ----------------------------------------------- | ------------------------ | --------------- |
| GET    | `/`                                             | Search resolvers         | Public          |
| GET    | `/id/{userId}/{resolverId:guid}`                | Get by ID                | Public          |
| GET    | `/slug/{username}/{name}`                       | Get by slug              | Public          |
| POST   | `/id/{userId}`                                  | Create resolver          | Owner only      |
| PUT    | `/id/{userId}/{resolverId}`                     | Update resolver metadata | Owner only      |
| DELETE | `/id/{userId}/{resolverId:guid}`                | Delete resolver          | Admin only      |
| POST   | `/slug/{username}/{name}/like/{likerId}/{like}` | Like/unlike              | Liker only      |
| GET    | `/slug/{username}/{name}/versions`              | List versions            | Public          |
| GET    | `/slug/{username}/{name}/versions/latest`       | Get latest version       | Public          |
| GET    | `/slug/{username}/{name}/versions/{ver}`        | Get specific version     | Public          |
| POST   | `/slug/{username}/{name}/versions`              | Create version           | Owner only      |
| PUT    | `/id/{userId}/{resolverId}/versions/{ver}`      | Update version metadata  | Owner only      |
| POST   | `/push/{username}`                              | Push (upsert)            | Owner only      |
| GET    | `/versions/{versionId:guid}`                    | Get version by ID        | Public          |

---

## Implementation Details

### Domain Models

**Resolver.cs:**

```csharp
ResolverSearch(owner?, search?, limit, skip)
Resolver(Principal, User, Versions, Info)
ResolverPrincipal(Id, UserId, Metadata, Record)
ResolverInfo(Downloads, Dependencies, Stars)  // all uint
ResolverRecord(Name)
ResolverMetadata(Project, Source, Email, Tags, Description, Readme)
```

**ResolverVersion.cs:**

```csharp
ResolverVersionSearch(search?, limit, skip)
ResolverVersion(Principal, ResolverPrincipal)
ResolverVersionRef(Username, Name, Version?)
ResolverVersionPrincipal(Id, Version, CreatedAt, Record, Property)
ResolverVersionRecord(Description)
ResolverVersionProperty(DockerReference, DockerTag)
```

### Data Models

**ResolverData.cs:**

```csharp
Id (Guid, PK), UserId (string, FK), Name, Project, Source, Email,
Tags[], Description, Readme, Downloads, Dependencies,
SearchVector (tsvector), Versions, Likes
```

**ResolverVersionData.cs:**

```csharp
Id (Guid, PK), ResolverId (Guid, FK), Version (ulong),
Description, DockerReference, DockerTag, CreatedAt
```

**ResolverLikeData.cs:**

```csharp
UserId (string, FK), ResolverId (Guid, FK)
```

**TemplateResolverVersionData.cs (junction):**

```csharp
Id (Guid, PK), TemplateId (Guid, FK), ResolverId (Guid, FK),
Template (navigation), Resolver (navigation)
```

### Database Configuration

- **Resolvers:** Unique index on (UserId, Name), tsvector GIN index for search
- **ResolverVersions:** Unique index on (Id, Version)
- **ResolverLikes:** Unique index on (UserId, ResolverId)
- **TemplateResolverVersions:** Junction table for template-resolver dependencies

### Request/Response DTOs

```csharp
// Requests
CreateResolverReq(Name, Project, Source, Email, Tags[], Description, Readme)
UpdateResolverReq(Project, Source, Email, Tags[], Description, Readme)
CreateResolverVersionReq(Description, DockerReference, DockerTag)
UpdateResolverVersionReq(Description)  // Only metadata!
PushResolverReq(Name, Project, Source, Email, Tags[], Description, Readme,
                VersionDescription, DockerReference, DockerTag)

// Responses
ResolverPrincipalResp, ResolverInfoResp, ResolverResp
ResolverVersionPrincipalResp, ResolverVersionResp
```

---

## Template-Resolver Dependencies

Templates can depend on resolvers, following the same pattern as processors and plugins.

### Changes Required

| File                                                       | Change                                                       |
| ---------------------------------------------------------- | ------------------------------------------------------------ |
| `Domain/Model/TemplateVersion.cs`                          | Add `IEnumerable<ResolverVersionPrincipal> Resolvers`        |
| `App/Modules/Cyan/Data/Models/TemplateVersionData.cs`      | Add `IEnumerable<TemplateResolverVersionData> Resolvers`     |
| `Domain/Repository/ITemplateRepository.cs`                 | Add resolver IDs to CreateVersion methods                    |
| `App/Modules/Cyan/Data/Repositories/TemplateRepository.cs` | Handle resolver junction table                               |
| `Domain/Service/TemplateService.cs`                        | Add `IResolverRepository` dependency, validate resolver refs |
| `App/StartUp/Database/MainDbContext.cs`                    | Add DbSet, configure relationships                           |

### Backward Compatibility

- Older templates (without resolver dependencies) return empty `Resolvers` array
- `CreateVersion` and `Push` accept optional resolver references (default to empty)
- Database queries handle missing junction records gracefully

---

## Files to Create/Modify

### New Files (18)

1. `Domain/Model/Resolver.cs`
2. `Domain/Model/ResolverVersion.cs`
3. `Domain/Repository/IResolverRepository.cs`
4. `Domain/Service/IResolverService.cs`
5. `Domain/Service/ResolverService.cs`
6. `App/Modules/Cyan/Data/Models/ResolverData.cs`
7. `App/Modules/Cyan/Data/Models/ResolverVersionData.cs`
8. `App/Modules/Cyan/Data/Models/ResolverLikeData.cs`
9. `App/Modules/Cyan/Data/Models/TemplateResolverData.cs`
10. `App/Modules/Cyan/Data/Repositories/ResolverRepository.cs`
11. `App/Modules/Cyan/API/V1/Controllers/ResolverController.cs`
12. `App/Modules/Cyan/API/V1/Models/ResolverModel.cs`
13. `App/Modules/Cyan/API/V1/Models/ResolverVersionModel.cs`
14. `App/Modules/Cyan/API/V1/Mappers/ResolverMapper.cs`
15. `App/Modules/Cyan/API/V1/Validators/ResolverValidator.cs`
16. `App/Modules/Cyan/API/V1/Validators/ResolverVersionValidator.cs`
17. `docs/developer/features/06-resolver-registry.md`
18. `docs/developer/surfaces/api/06-resolver.md`

### Modified Files (8)

1. `App/StartUp/Database/MainDbContext.cs`
2. `Domain/Model/TemplateVersion.cs`
3. `App/Modules/Cyan/Data/Models/TemplateVersionData.cs`
4. `Domain/Repository/ITemplateRepository.cs`
5. `App/Modules/Cyan/Data/Repositories/TemplateRepository.cs`
6. `Domain/Service/TemplateService.cs`
7. `docs/README.md`
8. `docs/developer/concepts/05-dependency.md`

### Generated Files

1. Database migration via `pls migration:create -- AddResolvers`

---

## Reference Files

| What            | Reference File                                               |
| --------------- | ------------------------------------------------------------ |
| Domain model    | `Domain/Model/Processor.cs`, `ProcessorVersion.cs`           |
| Repository      | `Domain/Repository/IProcessorRepository.cs`                  |
| Service         | `Domain/Service/ProcessorService.cs`, `IProcessorService.cs` |
| Data model      | `App/Modules/Cyan/Data/Models/ProcessorData.cs`              |
| API endpoints   | `App/Modules/Cyan/API/V1/Controllers/ProcessorController.cs` |
| API models      | `App/Modules/Cyan/API/V1/Models/ProcessorModel.cs`           |
| Validators      | `App/Modules/Cyan/API/V1/Validators/ProcessorValidator.cs`   |
| Mappers         | `App/Modules/Cyan/API/V1/Mappers/ProcessorMapper.cs`         |
| Junction table  | `App/Modules/Cyan/Data/Models/TemplateProcessorData.cs`      |
| Feature docs    | `docs/developer/features/04-processor-registry.md`           |
| API docs        | `docs/developer/surfaces/api/02-processor.md`                |
| Dependency docs | `docs/developer/concepts/05-dependency.md`                   |

---

## Edge Cases & Error Handling

| Case                                | Response                  |
| ----------------------------------- | ------------------------- |
| Duplicate resolver name (same user) | 409 Conflict              |
| Resolver not found                  | 404 Not Found             |
| User mismatch on create/update      | 401 Unauthorized          |
| Version not found                   | 404 Not Found             |
| Like race condition                 | 500 Internal Server Error |
| Validation failure                  | 400 Bad Request           |
| Admin required (delete)             | 403 Forbidden             |

---

## Commands

```bash
# Build
pls build

# Create migration
pls migration:create -- AddResolvers

# Run dev server (http://localhost:9001)
pls dev

# Pre-commit check
pre-commit run --all
```
