# Features Overview

This section documents the complex services and capabilities provided by Zinc.

## Features Map

```mermaid
flowchart TB
    subgraph Authentication
        JWT[JWT Authentication]
        APIKey[API Key Authentication]
    end

    subgraph Authorization
        Policies[Scope Policies]
    end

    subgraph Registries
        Template[Template Registry]
        Processor[Processor Registry]
        Plugin[Plugin Registry]
    end

    subgraph Features
        Version[Versioning]
        Search[Full-Text Search]
        Like[Like System]
        Deps[Dependency Resolution]
    end

    JWT --> Authorization
    APIKey --> Authorization

    Template --> Version
    Template --> Search
    Template --> Like
    Template --> Deps

    Processor --> Version
    Processor --> Search
    Processor --> Like

    Plugin --> Version
    Plugin --> Search
    Plugin --> Like
```

## Feature Index

| Feature | What | Why | Key Files |
|---------|------|------|-----------|
| [Authentication](./01-authentication.md) | Dual JWT + API Key auth | Interactive + automation | `App/StartUp/Services/AuthService.cs` |
| [Authorization](./02-authorization.md) | Scope-based policies | Flexible access control | `App/StartUp/Services/Auth/HasAnyHandler.cs` |
| [Template Registry](./03-template-registry.md) | Template CRUD + versions | Pipeline definitions | `Domain/Service/TemplateService.cs` |
| [Processor Registry](./04-processor-registry.md) | Processor CRUD + versions | Data processors | `Domain/Service/ProcessorService.cs` |
| [Plugin Registry](./05-plugin-registry.md) | Plugin CRUD + versions | Extensible features | `Domain/Service/PluginService.cs` |
| [Full-Text Search](./06-full-text-search.md) | PostgreSQL tsvector search | Fast text queries | `App/Modules/Cyan/Data/Repositories/TemplateRepository.cs:20-64` |
| [Like System](./07-like-system.md) | User bookmarks + counts | Discovery & popularity | `App/Modules/Cyan/Data/Repositories/TemplateRepository.cs:258-339` |
| [Token Management](./08-token-management.md) | API token lifecycle | Service authentication | `Domain/Service/TokenService.cs` |

## Feature Relationships

### Authentication Flow

```mermaid
sequenceDiagram
    participant Client
    participant Auth as Auth Middleware
    participant API as API Layer

    Client->>Auth: Request + Credentials
    Auth->>Auth: Validate JWT or API Key
    Auth->>API: Authenticated Context
    API-->>Client: Response
```

**See**: [Authentication](./01-authentication.md)

### Registry Operations

```mermaid
flowchart TB
    Create[Create] --> Update[Update Metadata]
    Create --> Version[Create Version]
    Update --> Delete[Delete]
    Version --> Like[Like/Unlike]
    Version --> Search[Search]
```

**See**: [Template Registry](./03-template-registry.md)

### Dependency Resolution

```mermaid
flowchart TB
    CreateVersion[Create Version] --> Validate[Validate Dependencies]
    Validate -->|All found| Success[Create Version]
    Validate -->|Any missing| Error[Return Error]
```

**See**: [Dependency Resolution Algorithm](../algorithms/01-dependency-resolution.md)

## How Features Work Together

### Template Creation Flow

```mermaid
sequenceDiagram
    participant User
    participant Auth as Auth Service
    participant Template as Template Service
    participant Repo as Template Repository
    participant Deps as Dependency Resolution

    User->>Auth: Authenticate
    Auth-->>User: User ID

    User->>Template: Create Template
    Template->>Repo: Insert Template
    Repo-->>Template: Template ID

    User->>Template: Create Version
    Template->>Deps: Validate Dependencies
    Deps-->>Template: Valid

    Template->>Repo: Insert Version
    Repo-->>Template: Version Number
    Template-->>User: Success
```

### Search Flow

```mermaid
sequenceDiagram
    participant User
    participant API as API Controller
    participant Service as Template Service
    participant Repo as Template Repository
    participant DB as PostgreSQL

    User->>API: Search Query
    API->>Service: Search(search)
    Service->>Repo: Search(search)
    Repo->>DB: SELECT with tsvector
    DB-->>Repo: Ranked Results
    Repo-->>Service: Template Principals
    Service-->>API: Results
    API-->>User: JSON Response
```

**See**: [Full-Text Search](./06-full-text-search.md)

## Related Sections

- [Concepts](../concepts/) - Domain terminology
- [Modules](../modules/) - Code organization
- [Algorithms](../algorithms/) - Implementation details
- [API](../surfaces/api/) - HTTP endpoints
