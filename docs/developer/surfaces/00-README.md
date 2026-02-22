# Surfaces

**What**: External interfaces exposed by Zinc.

**Why**: Documents how to interact with the API programmatically.

## Overview

Zinc exposes a REST API for managing templates, processors, plugins, users, and system operations.

- **[Template API](./api/01-template.md)** - Template CRUD, versions, search, and like operations
- **[Processor API](./api/02-processor.md)** - Processor CRUD, versions, and search
- **[Plugin API](./api/03-plugin.md)** - Plugin CRUD, versions, and search
- **[User API](./api/04-user.md)** - User management and token operations
- **[System API](./api/05-system.md)** - Health and system information

## Authentication

All endpoints except health checks require authentication via:

1. **JWT Bearer Token** - From Descope OAuth via JWKS
2. **API Key** - Via `X-API-TOKEN` header

See [Authentication Feature](../features/01-authentication.md) for details.

## Authorization

Authorization is enforced using scope-based policies:

- **`HasAny`** - User has any of the required scopes
- **`HasAll`** - User has all of the required scopes

See [Authorization Feature](../features/02-authorization.md) for details.

## Common Response Codes

| Code | Description |
|------|-------------|
| `200 OK` | Request succeeded |
| `201 Created` | Resource created successfully |
| `204 No Content` | Success with no response body |
| `400 Bad Request` | Validation failed |
| `401 Unauthorized` - Authentication required or failed |
| `403 Forbidden` - User lacks required scope |
| `404 Not Found` - Resource not found |
| `409 Conflict` - Resource already exists or state conflict |
| `500 Internal Server Error` - Server error |

## Error Format

Errors follow a consistent format:

```json
{
  "type": "ErrorType",
  "message": "Human-readable error message",
  "details": {}
}
```

**Key File**: `App/Error/V1/`

## Pagination

List endpoints support pagination:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `skip` | int | 0 | Number of results to skip |
| `limit` | int | 50 | Maximum number of results to return |

## Full-Text Search

Search endpoints support full-text search using PostgreSQL's `tsvector` and GIN indexes.

**Key File**: `App/Modules/Cyan/Data/Repositories/` (all repositories)

See [Full-Text Search](../features/06-full-text-search.md) for details.
