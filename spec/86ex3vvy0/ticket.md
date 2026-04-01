# Ticket: 86ex3vvy0

- **Type**: ClickUp task
- **Status**: in progress
- **URL**: https://app.clickup.com/t/86ex3vvy0
- **Parent**: none

## Description

## Problem

When users push new templates, processors, plugins, or resolvers via the `POST /api/v1/{entity}/push/{username}` endpoints, the `docs/README.md` is **not** updated to reflect the changes. READMEs are currently static and must be updated manually.

## Proposed Solution

Add automated README generation/update logic to the push service methods:

- `TemplateService.Push()`
- `PluginService.Push()`
- `ProcessorService.Push()`
- `ResolverService.Push()`

## Scope

- README should document available entities, versions, descriptions, and docker references
- Decide on strategy: append-only updates vs. full regeneration on each push
- Consider formatting conventions and section structure
- Ensure the README stays in sync with the actual registry state

## Context

- Push operations currently only update the database (metadata + version entries)
- No file write or markdown generation exists in the codebase today

## Comments

_No comments available from `cup task --json` output._
