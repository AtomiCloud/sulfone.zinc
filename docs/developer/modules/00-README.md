# Modules Overview

This section documents the code organization and structure of the Zinc application.

## Module Map

```mermaid
flowchart TB
    subgraph Application
        Startup[StartUp]
        Cyan[Cyan]
        Users[Users]
        System[System]
        Common[Common]
    end

    subgraph Domain
        Model[Domain Models]
        Service[Domain Services]
        Repository[Repository Interfaces]
    end

    subgraph Data
        DataModels[Data Models]
        Repositories[Repository Implementations]
        Mappers[Mappers]
    end

    subgraph API
        Controllers[Controllers]
        Validators[Validators]
        MappersAPI[API Mappers]
    end

    Startup --> Config[Configuration]
    Startup --> DI[Dependency Injection]

    Cyan --> API
    Users --> API
    System --> API

    API --> Service
    Service --> Repository
    Repository --> Repositories
    Repositories --> DataModels
```

## Module Index

| Module | Purpose | Key Files | Dependencies |
|--------|---------|-----------|--------------|
| [StartUp](./01-startup.md) | Configuration, DI setup | `App/StartUp/` | All modules |
| [Cyan](./02-cyan.md) | Templates, Processors, Plugins | `App/Modules/Cyan/` | Domain, Users |
| [Users](./03-users.md) | User management, tokens | `App/Modules/Users/` | Domain |
| [System](./04-system.md) | Health, error handling | `App/Modules/System/` | Common, Domain |
| [Common](./05-common.md) | Shared components | `App/Modules/Common/` | None |

## Layer Architecture

```mermaid
flowchart TB
    subgraph Layers
        API[API Layer<br/>Controllers + Validators]
        Domain[Domain Layer<br/>Services + Interfaces]
        Data[Data Layer<br/>Repositories + DbContext]
    end

    API -->|Calls| Domain
    Domain -->|Uses| Data
    Data -->|Queries| DB[(PostgreSQL)]
```

## File Organization

```text
App/
├── StartUp/              # Configuration and DI
│   ├── Services/         # Service configurations
│   ├── Registry/         # Component registries
│   ├── Options/          # Configuration options
│   └── Database/         # Database setup
│
├── Modules/
│   ├── Cyan/             # Templates, Processors, Plugins
│   │   ├── API/          # Controllers and validators
│   │   └── Data/         # Repositories and models
│   ├── Users/            # User management
│   │   ├── API/          # Controllers and validators
│   │   └── Data/         # Repositories and models
│   ├── System/           # System health
│   │   └── API/          # Controllers
│   └── Common/           # Shared components
│       └── API/          # Base controllers
│
├── Error/                # Error types
└── Migrations/           # Database migrations

Domain/
├── Model/                # Domain models
├── Service/              # Business logic
└── Repository/           # Repository interfaces
```

## Module Relationships

```mermaid
flowchart LR
    Startup[StartUp] -->|Configures| Cyan[Cyan]
    Startup -->|Configures| Users[Users]
    Startup -->|Configures| System[System]

    Cyan -->|Uses| Domain[Domain]
    Users -->|Uses| Domain
    System -->|Uses| Common[Common]

    Cyan -->|References| Users
```

## Responsibilities

| Module | Responsibilities |
|--------|-----------------|
| **StartUp** | App initialization, DI configuration, middleware setup |
| **Cyan** | Template/Processor/Plugin CRUD, versioning, search |
| **Users** | User CRUD, token management, authentication |
| **System** | Health checks, error documentation |
| **Common** | Shared controller base classes, utilities |

## Key Interfaces

| Interface | Implementations | Purpose |
|-----------|----------------|---------|
| `ITemplateService` | `TemplateService` | Template business logic |
| `IProcessorService` | `ProcessorService` | Processor business logic |
| `IPluginService` | `PluginService` | Plugin business logic |
| `IUserService` | `UserService` | User business logic |
| `ITokenService` | `TokenService` | Token business logic |

## Related Sections

- [Features](../features/) - Functional capabilities
- [Concepts](../concepts/) - Domain terminology
- [Algorithms](../algorithms/) - Implementation details
