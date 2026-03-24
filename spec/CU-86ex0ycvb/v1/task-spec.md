# Task Specification: [Zn] Store sub-template preset answer configs in registry (CU-86ex0ycvb)

## Source

- Ticket: CU-86ex0ycvb
- System: ClickUp
- URL: https://app.clickup.com/t/86ex0ycvb

## Summary

Extend the `TemplateTemplateVersionData` junction table to store preset answer configs alongside sub-template dependency declarations. When a template version is published with sub-template dependencies, its preset answers (key-value pairs like `atomi/platform=ketone`) are persisted in Zinc so consumers can skip re-prompting for pre-set values.

## Acceptance Criteria

- [ ] `TemplateTemplateVersionData` has a `PresetAnswers` column (JSON text, non-nullable, default `{}`)
- [ ] API request models (`CreateTemplateVersionReq`, `PushTemplateReq`) accept `PresetAnswers` on sub-template references
- [ ] API response includes preset answers for each sub-template via a new `TemplateVersionTemplateRefResp` type
- [ ] Domain model `TemplateVersion` exposes preset answers through sub-template references
- [ ] EF Core migration adds the `PresetAnswers` column with default `{}`
- [ ] Existing sub-template rows without preset answers get `{}` as default

## Out of Scope

- Injecting preset answers into dependency tree resolution (Ir task)
- Modifying cyan.yaml schema or Iridium's publish flow
- Validation of preset answer keys/values against sub-template prompts

## Constraints

- Follow the resolver config pattern: `JsonElement` in domain, `string` (JSON text) in data layer, `Dictionary<string, string>` in API requests
- `PresetAnswers` is non-nullable with default `{}` — always present in requests and responses
- Position-based matching for sub-template links (same pattern as resolver links after PR #49)
- PostgreSQL TEXT column for storage (no JSONB indexing needed)

## Context

This is the Zinc (registry) half of the "Preset answers for sub-templates" feature (parent: 86ex0ybx9). The Ir half handles pushing configs during publish and injecting them into dependency resolution. Zinc only needs to store and serve them.

The existing resolver feature (`TemplateResolverVersionData`) established the pattern for storing per-reference config:

- Data layer: `string Config` (JSON text)
- Domain layer: `JsonElement Config`
- API: `JsonElement Config` on request, included in response

Preset answers follow the same shape but are specifically `Dictionary<string, string>` (simpler than the generic `JsonElement` resolver config).

## Edge Cases

- **Sub-template with no presets**: `PresetAnswers` defaults to `{}`, not null — API always returns the field
- **Duplicate sub-template references**: Supported (same pattern as PR #49 resolver dedup) — matched by position, not identity
- **Empty preset answers object**: Valid — `{}` means "no presets for this sub-template"

## Implementation Checklist

### Testing

- [ ] Unit tests for data mapper (serialize/deserialize preset answers)
- [ ] Unit tests for API mapper (request → domain, domain → response)
- [ ] Integration test for API create/push with preset answers

**Test location:** `UnitTest/` for unit tests, `IntTest/` for integration tests

### Linting

- [ ] Run `dotnet build` to verify compilation
- [ ] Run `dotnet test` to verify all tests pass

### Notes

- Build: `pls build` (or `direnv exec . pls build`)
- Migration: `pls migration:create <name>`
- Test: `pls test` (or `direnv exec . dotnet test`)
