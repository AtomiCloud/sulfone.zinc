# System API

<!--
NOTE: The System controllers are located at App/Modules/System/ rather than App/Modules/System/API/V1/.
This is intentional - SystemController and V1ErrorController are infrastructure-level controllers
that don't follow the same versioning pattern as business modules (Cyan, Users). Moving them would
require code changes beyond documentation scope.
-->

**Base Path**: `/` and `/api/v1/error-info`
**Key File**: `App/Modules/System/SystemController.cs`, `App/Modules/System/V1ErrorController.cs`

## Endpoints

### System Info

```http
GET /
```

**Description**: Get system information and health status.

**Authorization**: Not required

**Response**: `200 OK`

```json
{
  "landscape": "lapras",
  "platform": "...",
  "service": "...",
  "module": "...",
  "version": "...",
  "status": "OK",
  "timeStamp": "2024-01-01T00:00:00Z"
}
```

**Key File**: `SystemController.cs:14-30`

---

### Error List

```http
GET /api/v1/error-info
```

**Description**: Get list of all error type IDs.

**Authorization**: Not required

**Response**: `200 OK`

```json
["ENTITY_NOT_FOUND", "ALREADY_EXISTS", "UNAUTHORIZED", ...]
```

**Key File**: `V1ErrorController.cs:23-29`

---

### Error Details

```http
GET /api/v1/error-info/{id}
```

**Description**: Get detailed information about a specific error type including JSON schema.

**Authorization**: Not required

**Response**: `200 OK`

```json
{
  "schema": { ... },
  "id": "ENTITY_NOT_FOUND",
  "title": "Entity Not Found",
  "version": "1.0"
}
```

**Key File**: `V1ErrorController.cs:31-57`

---

## Error Types

| Code                  | Status | Description                              | Key File                   |
| --------------------- | ------ | ---------------------------------------- | -------------------------- |
| `ENTITY_NOT_FOUND`    | 404    | Requested entity doesn't exist           | `EntityNotFound.cs`        |
| `ALREADY_EXISTS`      | 409    | Entity with identity already exists      | `AlreadyExistException.cs` |
| `UNAUTHORIZED`        | 401    | Authentication required or failed        | `Unauthorized.cs`          |
| `LIKE_CONFLICT`       | 409    | Like/unlike conflicts with current state | `LikeConflict.cs`          |
| `LIKE_RACE_CONDITION` | 500    | Race condition in like operation         | `LikeRaceCondition.cs`     |
| `INVALID_EXCEPTION`   | 400    | Invalid request data                     | `InvalidException.cs`      |

**Key Directory**: `App/Error/V1/`

## Related

- [System Module](../../modules/04-system.md)
- [Common Module](../../modules/05-common.md) - Error handling
