# Zinc Developer Documentation

**Zinc** is the Registry Backend - an ASP.NET Core application that manages templates, processors, and plugins for the Atom CI platform.

## Quick Start

1. **New to Zinc?** Start with [Getting Started](./01-getting-started.md)
2. **Understanding the system**: See [Architecture](./02-architecture.md)
3. **Core concepts**: Read [Concepts](./concepts/)
4. **Feature details**: Explore [Features](./features/)
5. **API reference**: Check [Surfaces](./surfaces/)

## Documentation Structure

```text
docs/developer/
├── 00-README.md              # This file - entry point
├── 01-getting-started.md     # Setup & quickstart
├── 02-architecture.md        # High-level system overview
├── concepts/                 # Domain terminology
├── features/                 # Authentication, registries, search
├── modules/                  # Code organization (Cyan, Users, System)
├── surfaces/                 # API endpoints & contracts
└── algorithms/               # Implementation details
```

## Key Technologies

- **ASP.NET Core** - Web framework
- **PostgreSQL** - Database with full-text search
- **Entity Framework Core** - ORM
- **JWT + API Key** - Dual authentication scheme

## Main Features

| Feature | Description |
|---------|-------------|
| [Dual Authentication](./features/01-authentication.md) | JWT (Descope) + API Token authentication |
| [Scope-Based Authorization](./features/02-authorization.md) | HasAny/HasAll policy handlers |
| [Template Registry](./features/03-template-registry.md) | Template CRUD with versioning |
| [Processor Registry](./features/04-processor-registry.md) | Processor CRUD with versioning |
| [Plugin Registry](./features/05-plugin-registry.md) | Plugin CRUD with versioning |
| [Full-Text Search](./features/06-full-text-search.md) | PostgreSQL tsvector + GIN indexes |
| [Like System](./features/07-like-system.md) | User bookmarking with optimistic locking |
| [Token Management](./features/08-token-management.md) | API token lifecycle |
| [Dependency Resolution](./algorithms/01-dependency-resolution.md) | Validates cross-version references |

## Modules Overview

| Module | Purpose | Key Files |
|--------|---------|-----------|
| [StartUp](./modules/01-startup.md) | Configuration, DI setup | `App/StartUp/` |
| [Cyan](./modules/02-cyan.md) | Templates, Processors, Plugins | `App/Modules/Cyan/` |
| [Users](./modules/03-users.md) | User management, tokens | `App/Modules/Users/` |
| [System](./modules/04-system.md) | Health, error handling | `App/Modules/System/` |
| [Common](./modules/05-common.md) | Shared utilities, base controllers | `App/Modules/Common/` |

## Core Concepts

| Concept | Description |
|---------|-------------|
| [Authentication](./concepts/01-authentication.md) | Dual authentication scheme (JWT + API Key) |
| [Authorization](./concepts/02-authorization.md) | Scope-based HasAny/HasAll policies |
| [Registry](./concepts/03-registry.md) | Container for versioned artifacts |
| [Version](./concepts/04-version.md) | Auto-incrementing immutable versions |
| [Dependency](./concepts/05-dependency.md) | Cross-version references |

## External Links

- **Related Projects**: Copper (CLI), Iron (Orchestrator)
