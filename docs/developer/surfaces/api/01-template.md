# Template API

**Base Path**: `/api/v1/template`
**Key File**: `App/Modules/Cyan/API/V1/Controllers/TemplateController.cs`

## Endpoints

### Search Templates

```http
GET /api/v1/template?skip=0&limit=50&search=query&owner=username
```

**Description**: Search and list templates with pagination and full-text search.

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
    "name": "my-pipeline",
    "project": "my-project",
    "description": "My CI/CD pipeline",
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

**Key File**: `TemplateController.cs:38-49`

---

### Get Template by ID

```http
GET /api/v1/template/id/{userId}/{templateId}
```

**Description**: Get a specific template by user ID and template ID.

**Path Parameters**:

| Parameter    | Type   | Description |
| ------------ | ------ | ----------- |
| `userId`     | string | User ID     |
| `templateId` | Guid   | Template ID |

**Response**: `200 OK`

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "my-pipeline",
  "project": "my-project",
  "source": "github.com/user/repo",
  "email": "user@example.com",
  "description": "My CI/CD pipeline",
  "tags": ["ci", "docker"],
  "readme": "# Usage\n...",
  "downloads": 42,
  "stars": 5,
  "createdAt": "2024-01-01T00:00:00Z",
  "versions": [
    {
      "id": "version-guid",
      "version": 1,
      "description": "Initial version",
      "dockerImage": "my-registry/pipeline",
      "dockerTag": "v1"
    }
  ]
}
```

**Error**: `404 Not Found` if template doesn't exist

**Key File**: `TemplateController.cs:51-59`

---

### Get Template by Slug

```http
GET /api/v1/template/slug/{username}/{name}
```

**Description**: Get a template by username and name.

**Path Parameters**:

| Parameter  | Type   | Description   |
| ---------- | ------ | ------------- |
| `username` | string | Username      |
| `name`     | string | Template name |

**Response**: `200 OK` (same as Get by ID)

**Error**: `404 Not Found` if template doesn't exist

**Key File**: `TemplateController.cs:61-69`

---

### Create Template

```http
POST /api/v1/template/id/{userId}
```

**Description**: Create a new template.

**Path Parameters**:

| Parameter | Type   | Description                             |
| --------- | ------ | --------------------------------------- |
| `userId`  | string | User ID (must match authenticated user) |

**Request Body**:

```json
{
  "name": "my-pipeline",
  "project": "my-project",
  "source": "github.com/user/repo",
  "email": "user@example.com",
  "description": "My CI/CD pipeline",
  "tags": ["ci", "docker"],
  "readme": "# Usage\n..."
}
```

**Response**: `201 Created`

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "my-pipeline",
  "project": "my-project",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Errors**:

- `400 Bad Request` - Validation failed
- `401 Unauthorized` - User ID doesn't match authenticated user
- `409 Conflict` - Template name already exists

**Key File**: `TemplateController.cs:71-91`

---

### Update Template

```http
PUT /api/v1/template/id/{userId}/{templateId}
```

**Description**: Update template metadata (name cannot be changed).

**Path Parameters**:

| Parameter    | Type   | Description |
| ------------ | ------ | ----------- |
| `userId`     | string | User ID     |
| `templateId` | Guid   | Template ID |

**Request Body**:

```json
{
  "description": "Updated description",
  "tags": ["ci", "docker", "updated"],
  "readme": "# Updated Usage\n..."
}
```

**Response**: `200 OK`

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "my-pipeline",
  "project": "my-project",
  "description": "Updated description",
  "tags": ["ci", "docker", "updated"],
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Errors**:

- `400 Bad Request` - Validation failed
- `401 Unauthorized` - User ID doesn't match
- `404 Not Found` - Template doesn't exist

**Key File**: `TemplateController.cs:93-115`

---

### Delete Template

```http
DELETE /api/v1/template/id/{userId}/{templateId}
```

**Description**: Delete a template. Requires Admin scope.

**Path Parameters**:

| Parameter    | Type   | Description |
| ------------ | ------ | ----------- |
| `userId`     | string | User ID     |
| `templateId` | Guid   | Template ID |

**Authorization**: Requires `Admin` scope (via `AuthPolicies.OnlyAdmin` policy)

**Response**: `204 No Content`

**Errors**:

- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User lacks Admin scope
- `404 Not Found` - Template doesn't exist

**Key File**: `TemplateController.cs:149-157`

---

### Like Template

```http
POST /api/v1/template/slug/{username}/{templateName}/like/{likerId}/{like}
```

**Description**: Like or unlike a template.

**Path Parameters**:

| Parameter      | Type    | Description                                                         |
| -------------- | ------- | ------------------------------------------------------------------- |
| `username`     | string  | Username of template owner                                          |
| `templateName` | string  | Template name                                                       |
| `likerId`      | string  | User ID of the user liking/unliking (must match authenticated user) |
| `like`         | boolean | `true` to like, `false` to unlike                                   |

**Response**: `204 No Content`

**Errors**:

- `401 Unauthorized` - likerId doesn't match authenticated user
- `404 Not Found` - Template doesn't exist
- `409 Conflict` - Already liked (when liking) or not liked (when unliking)
- `500 Internal Server Error` - Race condition detected

**Key File**: `TemplateController.cs:119-147`

---

### Get Template Versions

```http
GET /api/v1/template/slug/{username}/{templateName}/versions?skip=0&limit=50&search=query
```

**Description**: List template versions.

**Path Parameters**:

| Parameter      | Type   | Description   |
| -------------- | ------ | ------------- |
| `username`     | string | Username      |
| `templateName` | string | Template name |

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
    "dockerImage": "my-registry/pipeline",
    "dockerTag": "v1",
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

**Key File**: `TemplateController.cs:159-171`

---

### Get Template Version by Number

```http
GET /api/v1/template/slug/{username}/{templateName}/versions/{ver}?bumpDownload=true
```

**Description**: Get a specific template version.

**Path Parameters**:

| Parameter      | Type   | Description    |
| -------------- | ------ | -------------- |
| `username`     | string | Username       |
| `templateName` | string | Template name  |
| `ver`          | ulong  | Version number |

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
  "dockerImage": "my-registry/pipeline",
  "dockerTag": "v1",
  "processors": [
    {
      "id": "processor-guid",
      "version": 2
    }
  ],
  "plugins": [
    {
      "id": "plugin-guid",
      "version": 1
    }
  ],
  "templates": [],
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Key File**: `TemplateController.cs:187-206`

---

### Get Latest Template Version

```http
GET /api/v1/template/slug/{username}/{templateName}/versions/latest?bumpDownload=true
```

**Description**: Get the latest template version.

**Path Parameters**:

| Parameter      | Type   | Description   |
| -------------- | ------ | ------------- |
| `username`     | string | Username      |
| `templateName` | string | Template name |

**Query Parameters**:

| Parameter      | Type    | Default | Description              |
| -------------- | ------- | ------- | ------------------------ |
| `bumpDownload` | boolean | false   | Increment download count |

**Response**: `200 OK` (same as Get Template Version by Number)

**Key File**: `TemplateController.cs:208-226`

---

### Create Template Version

```http
POST /api/v1/template/slug/{username}/{templateName}/versions
```

**Description**: Create a new template version.

**Path Parameters**:

| Parameter  | Type   | Description   |
| ---------- | ------ | ------------- |
| `username` | string | Username      |
| `name`     | string | Template name |

**Request Body**:

```json
{
  "description": "New version",
  "dockerImage": "my-registry/pipeline",
  "dockerTag": "v2",
  "processors": [
    {
      "processorId": "processor-guid",
      "version": 2
    }
  ],
  "plugins": [
    {
      "pluginId": "plugin-guid",
      "version": 1
    }
  ],
  "templates": []
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
- `404 Not Found` - Template or dependency not found

**Key File**: `TemplateController.cs:248-291`

---

### Get Template Versions by ID

```http
GET /api/v1/template/id/{userId}/{templateId:guid}/versions?skip=0&limit=50&search=query
```

**Description**: List template versions by template ID.

**Path Parameters**:

| Parameter    | Type   | Description |
| ------------ | ------ | ----------- |
| `userId`     | string | User ID     |
| `templateId` | Guid   | Template ID |

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
    "dockerImage": "my-registry/pipeline",
    "dockerTag": "v1",
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

**Key File**: `TemplateController.cs:173-185`

---

### Get Template Version by ID and Number

```http
GET /api/v1/template/id/{userId}/{templateId:guid}/versions/{ver}
```

**Description**: Get a specific template version by template ID.

**Path Parameters**:

| Parameter    | Type   | Description    |
| ------------ | ------ | -------------- |
| `userId`     | string | User ID        |
| `templateId` | Guid   | Template ID    |
| `ver`        | ulong  | Version number |

**Response**: `200 OK`

```json
{
  "id": "version-guid",
  "version": 1,
  "description": "Initial version",
  "dockerImage": "my-registry/pipeline",
  "dockerTag": "v1",
  "processors": [],
  "plugins": [],
  "templates": [],
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Key File**: `TemplateController.cs:228-246`

---

### Get Template Version by ID

```http
GET /api/v1/template/versions/{versionId:guid}
```

**Description**: Get a template version by its version ID.

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
  "dockerImage": "my-registry/pipeline",
  "dockerTag": "v1",
  "processors": [],
  "plugins": [],
  "templates": [],
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Key File**: `TemplateController.cs:404-416`

---

### Create Template Version by ID

```http
POST /api/v1/template/id/{userId}/{templateId:guid}/versions
```

**Description**: Create a new template version by template ID.

**Path Parameters**:

| Parameter    | Type   | Description                             |
| ------------ | ------ | --------------------------------------- |
| `userId`     | string | User ID (must match authenticated user) |
| `templateId` | Guid   | Template ID                             |

**Request Body**:

```json
{
  "description": "New version",
  "dockerImage": "my-registry/pipeline",
  "dockerTag": "v2",
  "processors": [
    {
      "processorId": "processor-guid",
      "version": 2
    }
  ],
  "plugins": [
    {
      "pluginId": "plugin-guid",
      "version": 1
    }
  ],
  "templates": []
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
- `404 Not Found` - Template or dependency not found

**Key File**: `TemplateController.cs:293-328`

---

### Update Template Version

```http
PUT /api/v1/template/id/{userId}/{templateId:guid}/versions/{ver}
```

**Description**: Update a template version metadata.

**Path Parameters**:

| Parameter    | Type   | Description                             |
| ------------ | ------ | --------------------------------------- |
| `userId`     | string | User ID (must match authenticated user) |
| `templateId` | Guid   | Template ID                             |
| `ver`        | ulong  | Version number                          |

**Request Body**:

```json
{
  "description": "Updated version description",
  "dockerImage": "my-registry/pipeline",
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
- `404 Not Found` - Template doesn't exist

**Key File**: `TemplateController.cs:330-356`

---

### Push Template

```http
POST /api/v1/template/push/{username}
```

**Description**: Create or update a template and create a new version in a single operation.

**Path Parameters**:

| Parameter  | Type   | Description                              |
| ---------- | ------ | ---------------------------------------- |
| `username` | string | Username (must match authenticated user) |

**Request Body**:

```json
{
  "name": "my-pipeline",
  "project": "my-project",
  "source": "github.com/user/repo",
  "email": "user@example.com",
  "description": "My CI/CD pipeline",
  "tags": ["ci", "docker"],
  "readme": "# Usage\n...",
  "version": {
    "description": "Initial version",
    "dockerImage": "my-registry/pipeline",
    "dockerTag": "v1"
  },
  "processors": [
    {
      "processorId": "processor-guid",
      "version": 2
    }
  ],
  "plugins": [
    {
      "pluginId": "plugin-guid",
      "version": 1
    }
  ],
  "templates": []
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
- `404 Not Found` - Dependency not found

**Key File**: `TemplateController.cs:358-402`

---

## Related

- [Template Registry Feature](../../features/03-template-registry.md)
- [Template Repository](../../../App/Modules/Cyan/Data/Repositories/TemplateRepository.cs)
