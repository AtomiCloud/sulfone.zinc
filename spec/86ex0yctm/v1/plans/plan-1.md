# Plan 1: Add Commands field across all layers

## Summary

Add `string[] Commands` to template version data model and propagate through every layer — domain, data, API, controllers, and tests. This is a single cohesive change across 16 files.

## Files to modify

### Data layer (3 files)

1. `App/Modules/Cyan/Data/Models/TemplateVersionData.cs` — add `string[] Commands` property with default `Array.Empty<string>()`
2. `App/StartUp/Database/MainDbContext.cs` — add Commands column configuration for TemplateVersionData entity
3. `App/Modules/Cyan/Data/Mappers/TemplateMapper.cs` — map Commands in `ToPrincipal`, `ToDomain`, `HydrateData`

### Domain layer (3 files)

4. `Domain/Model/TemplateVersion.cs` — add `string[] Commands` to `TemplateVersionPrincipal`
5. `Domain/Repository/ITemplateRepository.cs` — add `string[] Commands` parameter to `CreateVersion` methods
6. `Domain/Service/ITemplateService.cs` — add `string[] Commands` parameter to `CreateVersion` and `Push` methods
7. `Domain/Service/TemplateService.cs` — propagate Commands through `CreateVersion` and `Push` implementations

### Repository implementation (1 file)

8. `App/Modules/Cyan/Data/Repositories/TemplateRepository.cs` — store Commands in TemplateVersionData in both `CreateVersion` overloads

### API layer (4 files)

9. `App/Modules/Cyan/API/V1/Models/TemplateVersionModel.cs` — add `string[] Commands` to `CreateTemplateVersionReq`, `PushTemplateReq`, `TemplateVersionPrincipalResp`, `TemplateVersionResp`
10. `App/Modules/Cyan/API/V1/Mappers/TemplateMapper.cs` — map Commands in `ToDomain`, `ToResp` methods for both API and version mappers
11. `App/Modules/Cyan/API/V1/Validators/TemplateVersionValidator.cs` — add validation: non-null array, each element non-empty/whitespace
12. `App/Modules/Cyan/API/V1/Controllers/TemplateController.cs` — pass Commands from request to service in `CreateVersion` and `Push` endpoints

### Tests (1 file)

13. `UnitTest/TemplateVersionMapperTests.cs` — add tests for Commands mapping through API and data layers

### Migration (generated)

14. Run `pls migration:create -- AddCommandsToTemplateVersions` — generates migration adding `text[]` column with `[]` default

### Verification

15. Run `pls dev` — start local server
16. Test endpoints: create version, push, get version — verify Commands appears in all responses
17. Run `pre-commit run --all` — ensure all checks pass
