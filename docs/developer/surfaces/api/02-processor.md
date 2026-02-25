# Processor API

**Base Path**: `/api/v1/processor`
**Key File**: `App/Modules/Cyan/API/V1/Controllers/ProcessorController.cs`

## Endpoints

### Search Processors

```http
GET /api/v1/processor?skip=0&limit=50&search=query&owner=username
```

**Description**: Search and list processors with pagination and full-text search.

**Query Parameters**:

| Parameter | Type   | Required | Description                  |
| --------- | ------ | -------- | ---------------------------- |
| `skip`    | int    | No       | Results to skip (default: 0) |
| `limit`   | int    | No       | Max results (default: 50)    |
| `search`  | string | No       | Full-text search query       |
| `owner`   | string | No       | Filter by username           |

**Response**: `200 OK`

```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "data-transform",
    "project": "my-project",
    "description": "Data transformation processor",
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

**Key File**: `ProcessorController.cs:37-48`

---

### Get Processor by ID

```http
GET /api/v1/processor/id/{userId}/{processorId:guid}
```

**Description**: Get a specific processor by user ID and processor ID.

**Path Parameters**:

| Parameter     | Type   | Description  |
| ------------- | ------ | ------------ |
| `userId`      | string | User ID      |
| `processorId` | Guid   | Processor ID |

**Response**: `200 OK`

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "data-transform",
  "project": "my-project",
  "source": "github.com/user/processor",
  "email": "user@example.com",
  "description": "Data transformation processor",
  "tags": ["data", "transform"],
  "readme": "# Usage\n...",
  "downloads": 10,
  "stars": 2,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Error**: `404 Not Found` if processor doesn't exist

**Key File**: `ProcessorController.cs:50-58`

---

### Get Processor by Slug

```http
GET /api/v1/processor/slug/{username}/{name}
```

**Description**: Get a processor by username and name.

**Path Parameters**:

| Parameter  | Type   | Description    |
| ---------- | ------ | -------------- |
| `username` | string | Username       |
| `name`     | string | Processor name |

**Response**: `200 OK` (same as Get by ID)

**Error**: `404 Not Found` if processor doesn't exist

**Key File**: `ProcessorController.cs:60-68`

---

### Create Processor

```http
POST /api/v1/processor/id/{userId}
```

**Description**: Create a new processor.

**Path Parameters**:

| Parameter | Type   | Description                             |
| --------- | ------ | --------------------------------------- |
| `userId`  | string | User ID (must match authenticated user) |

**Request Body**:

```json
{
  "name": "data-transform",
  "project": "my-project",
  "source": "github.com/user/processor",
  "email": "user@example.com",
  "description": "Data transformation processor",
  "tags": ["data", "transform"],
  "readme": "# Usage\n..."
}
```

**Response**: `201 Created`

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "data-transform",
  "project": "my-project",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Errors**:

<!--
NOTE: The 401 Unauthorized response for user ID mismatch matches the current controller implementation.
While HTTP semantics might suggest 403 Forbidden for an authenticated-but-unauthorized user, the docs
accurately reflect the current code behavior. Changing this would require code modifications and could
break existing API consumers.
-->

- `400 Bad Request` - Validation failed
- `401 Unauthorized` - User ID doesn't match authenticated user
- `409 Conflict` - Processor name already exists

**Key File**: `ProcessorController.cs:70-90`

---

### Update Processor

```http
PUT /api/v1/processor/id/{userId}/{processorId}
```

**Description**: Update processor metadata (name cannot be changed).

**Path Parameters**:

| Parameter     | Type   | Description  |
| ------------- | ------ | ------------ |
| `userId`      | string | User ID      |
| `processorId` | Guid   | Processor ID |

**Request Body**:

```json
{
  "description": "Updated description",
  "tags": ["data", "transform", "updated"],
  "readme": "# Updated Usage\n..."
}
```

**Response**: `200 OK`

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "data-transform",
  "project": "my-project",
  "description": "Updated description",
  "tags": ["data", "transform", "updated"],
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Errors**:

- `400 Bad Request` - Validation failed
- `401 Unauthorized` - User ID doesn't match
- `404 Not Found` - Processor doesn't exist

**Key File**: `ProcessorController.cs:92-116`

---

### Delete Processor

```http
DELETE /api/v1/processor/id/{userId}/{processorId:guid}
```

**Description**: Delete a processor. Requires Admin scope.

**Path Parameters**:

| Parameter     | Type   | Description  |
| ------------- | ------ | ------------ |
| `userId`      | string | User ID      |
| `processorId` | Guid   | Processor ID |

**Authorization**: Requires `Admin` scope (via `AuthPolicies.OnlyAdmin` policy)

**Response**: `204 No Content`

**Errors**:

- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User lacks Admin scope
- `404 Not Found` - Processor doesn't exist

**Key File**: `ProcessorController.cs:148-160`

---

### Like Processor

```http
POST /api/v1/processor/slug/{username}/{processorName}/like/{likerId}/{like}
```

**Description**: Like or unlike a processor.

**Path Parameters**:

| Parameter       | Type    | Description                                                         |
| --------------- | ------- | ------------------------------------------------------------------- |
| `username`      | string  | Username of processor owner                                         |
| `processorName` | string  | Processor name                                                      |
| `likerId`       | string  | User ID of the user liking/unliking (must match authenticated user) |
| `like`          | boolean | `true` to like, `false` to unlike                                   |

**Response**: `204 No Content`

**Errors**:

- `401 Unauthorized` - likerId doesn't match authenticated user
- `404 Not Found` - Processor doesn't exist
- `409 Conflict` - Already liked (when liking) or not liked (when unliking)
- `500 Internal Server Error` - Race condition detected

**Key File**: `ProcessorController.cs:118-146`

---

### Get Processor Versions

```http
GET /api/v1/processor/slug/{username}/{processorName}/versions?skip=0&limit=50&search=query
```

**Description**: List processor versions.

**Path Parameters**:

| Parameter       | Type   | Description    |
| --------------- | ------ | -------------- |
| `username`      | string | Username       |
| `processorName` | string | Processor name |

**Query Parameters**:

| Parameter | Type   | Required | Description                  |
| --------- | ------ | -------- | ---------------------------- |
| `skip`    | int    | No       | Results to skip (default: 0) |
| `limit`   | int    | No       | Max results (default: 50)    |
| `search`  | string | No       | Filter by description        |

**Response**: `200 OK`

```json
[
  {
    "id": "version-guid",
    "version": 1,
    "description": "Initial version",
    "dockerImage": "my-registry/processor",
    "dockerTag": "v1",
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

**Key File**: `ProcessorController.cs:162-174`

---

### Get Processor Versions by ID

```http
GET /api/v1/processor/id/{userId}/{processorId:guid}/versions?skip=0&limit=50&search=query
```

**Description**: List processor versions by processor ID.

**Path Parameters**:

| Parameter     | Type   | Description  |
| ------------- | ------ | ------------ |
| `userId`      | string | User ID      |
| `processorId` | Guid   | Processor ID |

**Query Parameters**:

| Parameter | Type   | Required | Description                  |
| --------- | ------ | -------- | ---------------------------- |
| `skip`    | int    | No       | Results to skip (default: 0) |
| `limit`   | int    | No       | Max results (default: 50)    |
| `search`  | string | No       | Filter by description        |

**Response**: `200 OK`

```json
[
  {
    "id": "version-guid",
    "version": 1,
    "description": "Initial version",
    "dockerImage": "my-registry/processor",
    "dockerTag": "v1",
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

**Key File**: `ProcessorController.cs:176-188`

---

### Get Processor Version by Number

```http
GET /api/v1/processor/slug/{username}/{processorName}/versions/{ver}?bumpDownload=true
```

**Description**: Get a specific processor version.

**Path Parameters**:

| Parameter       | Type   | Description    |
| --------------- | ------ | -------------- |
| `username`      | string | Username       |
| `processorName` | string | Processor name |
| `ver`           | ulong  | Version number |

**Query Parameters**:

| Parameter      | Type    | Default | Description              |
| -------------- | ------- | ------- | ------------------------ |
| `bumpDownload` | boolean | false   | Increment download count |

**Response**: `200 OK`

```json
{
  "id": "version-guid",
  "version": 1,
  "description": "Initial version",
  "dockerImage": "my-registry/processor",
  "dockerTag": "v1",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Key File**: `ProcessorController.cs:190-209`

---

### Get Latest Processor Version

```http
GET /api/v1/processor/slug/{username}/{processorName}/versions/latest?bumpDownload=true
```

**Description**: Get the latest processor version.

**Path Parameters**:

| Parameter       | Type   | Description    |
| --------------- | ------ | -------------- |
| `username`      | string | Username       |
| `processorName` | string | Processor name |

**Query Parameters**:

| Parameter      | Type    | Default | Description              |
| -------------- | ------- | ------- | ------------------------ |
| `bumpDownload` | boolean | false   | Increment download count |

**Response**: `200 OK` (same as Get Processor Version by Number)

**Key File**: `ProcessorController.cs:211-229`

---

### Get Processor Version by ID and Number

```http
GET /api/v1/processor/id/{userId}/{processorId:guid}/versions/{ver}
```

**Description**: Get a specific processor version by processor ID.

**Path Parameters**:

| Parameter     | Type   | Description    |
| ------------- | ------ | -------------- |
| `userId`      | string | User ID        |
| `processorId` | Guid   | Processor ID   |
| `ver`         | ulong  | Version number |

**Response**: `200 OK`

```json
{
  "id": "version-guid",
  "version": 1,
  "description": "Initial version",
  "dockerImage": "my-registry/processor",
  "dockerTag": "v1",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Key File**: `ProcessorController.cs:231-249`

---

### Get Processor Version by ID

```http
GET /api/v1/processor/versions/{versionId:guid}
```

**Description**: Get a processor version by its version ID.

**Path Parameters**:

| Parameter   | Type | Description |
| ----------- | ---- | ----------- |
| `versionId` | Guid | Version ID  |

**Response**: `200 OK`

```json
{
  "id": "version-guid",
  "version": 1,
  "description": "Initial version",
  "dockerImage": "my-registry/processor",
  "dockerTag": "v1",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Key File**: `ProcessorController.cs:393-405`

---

### Create Processor Version

```http
POST /api/v1/processor/slug/{username}/{processorName}/versions
```

**Description**: Create a new processor version.

**Path Parameters**:

| Parameter       | Type   | Description    |
| --------------- | ------ | -------------- |
| `username`      | string | Username       |
| `processorName` | string | Processor name |

**Request Body**:

```json
{
  "description": "New version",
  "dockerImage": "my-registry/processor",
  "dockerTag": "v2"
}
```

**Response**: `201 Created`

```json
{
  "id": "new-version-guid",
  "version": 2,
  "description": "New version",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Errors**:

- `400 Bad Request` - Validation failed
- `401 Unauthorized` - Username doesn't match authenticated user
- `404 Not Found` - Processor not found

**Key File**: `ProcessorController.cs:251-286`

---

### Create Processor Version by ID

```http
POST /api/v1/processor/id/{userId}/{processorId:guid}/versions
```

**Description**: Create a new processor version by processor ID.

**Path Parameters**:

| Parameter     | Type   | Description                             |
| ------------- | ------ | --------------------------------------- |
| `userId`      | string | User ID (must match authenticated user) |
| `processorId` | Guid   | Processor ID                            |

**Request Body**:

```json
{
  "description": "New version",
  "dockerImage": "my-registry/processor",
  "dockerTag": "v2"
}
```

**Response**: `201 Created`

```json
{
  "id": "new-version-guid",
  "version": 2,
  "description": "New version",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Errors**:

- `400 Bad Request` - Validation failed
- `401 Unauthorized` - User ID doesn't match
- `404 Not Found` - Processor not found

**Key File**: `ProcessorController.cs:288-319`

---

### Update Processor Version

```http
PUT /api/v1/processor/id/{userId}/{processorId:guid}/versions/{ver}
```

**Description**: Update a processor version metadata.

**Path Parameters**:

| Parameter     | Type   | Description                             |
| ------------- | ------ | --------------------------------------- |
| `userId`      | string | User ID (must match authenticated user) |
| `processorId` | Guid   | Processor ID                            |
| `ver`         | ulong  | Version number                          |

**Request Body**:

```json
{
  "description": "Updated version description",
  "dockerImage": "my-registry/processor",
  "dockerTag": "v2"
}
```

**Response**: `200 OK`

```json
{
  "id": "version-guid",
  "version": 2,
  "description": "Updated version description",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Errors**:

- `400 Bad Request` - Validation failed
- `401 Unauthorized` - User ID doesn't match
- `404 Not Found` - Processor doesn't exist

**Key File**: `ProcessorController.cs:321-351`

---

### Push Processor

```http
POST /api/v1/processor/push/{username}
```

**Description**: Create or update a processor and create a new version in a single operation.

**Path Parameters**:

| Parameter  | Type   | Description                              |
| ---------- | ------ | ---------------------------------------- |
| `username` | string | Username (must match authenticated user) |

**Request Body**:

```json
{
  "name": "data-transform",
  "project": "my-project",
  "source": "github.com/user/processor",
  "email": "user@example.com",
  "description": "Data transformation processor",
  "tags": ["data", "transform"],
  "readme": "# Usage\n...",
  "version": {
    "description": "Initial version",
    "dockerImage": "my-registry/processor",
    "dockerTag": "v1"
  }
}
```

**Response**: `201 Created`

```json
{
  "id": "new-version-guid",
  "version": 1,
  "description": "Initial version",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Errors**:

- `400 Bad Request` - Validation failed
- `401 Unauthorized` - Username doesn't match authenticated user
- `404 Not Found` - Referenced dependency (processor/plugin/template) not found during version creation

**Key File**: `ProcessorController.cs:353-391`

---

## Related

- [Processor Registry Feature](../../features/04-processor-registry.md)
- [Processor Repository](../../../../App/Modules/Cyan/Data/Repositories/ProcessorRepository.cs)
