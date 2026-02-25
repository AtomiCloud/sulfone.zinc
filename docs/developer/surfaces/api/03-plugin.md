# Plugin API

**Base Path**: `/api/v1/plugin`
**Key File**: `App/Modules/Cyan/API/V1/Controllers/PluginController.cs`

## Endpoints

### Search Plugins

```http
GET /api/v1/plugin?skip=0&limit=50&search=query&owner=username
```

**Description**: Search and list plugins with pagination and full-text search.

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
    "name": "notification-plugin",
    "project": "my-project",
    "description": "Notification plugin",
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

**Key File**: `PluginController.cs:37-48`

---

### Get Plugin by ID

```http
GET /api/v1/plugin/id/{userId}/{pluginId}
```

**Description**: Get a specific plugin by user ID and plugin ID.

**Path Parameters**:

| Parameter  | Type   | Description |
| ---------- | ------ | ----------- |
| `userId`   | string | User ID     |
| `pluginId` | Guid   | Plugin ID   |

**Response**: `200 OK`

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "notification-plugin",
  "project": "my-project",
  "source": "github.com/user/plugin",
  "email": "user@example.com",
  "description": "Notification plugin",
  "tags": ["notification", "plugin"],
  "readme": "# Usage\n...",
  "downloads": 15,
  "stars": 3,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Error**: `404 Not Found` if plugin doesn't exist

**Key File**: `PluginController.cs:50-58`

---

### Get Plugin by Slug

```http
GET /api/v1/plugin/slug/{username}/{name}
```

**Description**: Get a plugin by username and name.

**Path Parameters**:

| Parameter  | Type   | Description |
| ---------- | ------ | ----------- |
| `username` | string | Username    |
| `name`     | string | Plugin name |

**Response**: `200 OK` (same as Get by ID)

**Error**: `404 Not Found` if plugin doesn't exist

**Key File**: `PluginController.cs:60-68`

---

### Create Plugin

```http
POST /api/v1/plugin/id/{userId}
```

**Description**: Create a new plugin.

**Path Parameters**:

| Parameter | Type   | Description                             |
| --------- | ------ | --------------------------------------- |
| `userId`  | string | User ID (must match authenticated user) |

**Request Body**:

```json
{
  "name": "notification-plugin",
  "project": "my-project",
  "source": "github.com/user/plugin",
  "email": "user@example.com",
  "description": "Notification plugin",
  "tags": ["notification", "plugin"],
  "readme": "# Usage\n..."
}
```

**Response**: `201 Created`

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "notification-plugin",
  "project": "my-project",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Errors**:

<!--
NOTE: The 401 Unauthorized response when userId doesn't match is intentional and matches the current controller implementation (PluginController.cs:70-90).
While HTTP semantics might suggest 403 Forbidden for an authenticated-but-unauthorized user, changing this would be a breaking API change
and is outside the scope of documentation. The docs accurately reflect the current code behavior.
-->

- `400 Bad Request` - Validation failed
- `401 Unauthorized` - User ID doesn't match authenticated user
- `409 Conflict` - Plugin name already exists

**Key File**: `PluginController.cs:70-90`

---

### Update Plugin

```http
PUT /api/v1/plugin/id/{userId}/{pluginId}
```

**Description**: Update plugin metadata (name cannot be changed).

**Path Parameters**:

| Parameter  | Type   | Description |
| ---------- | ------ | ----------- |
| `userId`   | string | User ID     |
| `pluginId` | Guid   | Plugin ID   |

**Request Body**:

```json
{
  "description": "Updated description",
  "tags": ["notification", "plugin", "updated"],
  "readme": "# Updated Usage\n..."
}
```

**Response**: `200 OK`

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "notification-plugin",
  "project": "my-project",
  "description": "Updated description",
  "tags": ["notification", "plugin", "updated"],
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Errors**:

- `400 Bad Request` - Validation failed
- `401 Unauthorized` - User ID doesn't match
- `404 Not Found` - Plugin doesn't exist

**Key File**: `PluginController.cs:92-116`

---

### Delete Plugin

```http
DELETE /api/v1/plugin/id/{userId}/{pluginId}
```

**Description**: Delete a plugin. Requires Admin scope.

**Path Parameters**:

| Parameter  | Type   | Description |
| ---------- | ------ | ----------- |
| `userId`   | string | User ID     |
| `pluginId` | Guid   | Plugin ID   |

**Authorization**: Requires `Admin` scope (via `AuthPolicies.OnlyAdmin` policy)

**Response**: `204 No Content`

**Errors**:

- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User lacks Admin scope
- `404 Not Found` - Plugin doesn't exist

**Key File**: `PluginController.cs:140-148`

---

### Like Plugin

```http
POST /api/v1/plugin/slug/{username}/{pluginName}/like/{likerId}/{like}
```

**Description**: Like or unlike a plugin.

**Path Parameters**:

| Parameter    | Type    | Description                                                         |
| ------------ | ------- | ------------------------------------------------------------------- |
| `username`   | string  | Username of plugin owner                                            |
| `pluginName` | string  | Plugin name                                                         |
| `likerId`    | string  | User ID of the user liking/unliking (must match authenticated user) |
| `like`       | boolean | `true` to like, `false` to unlike                                   |

**Response**: `204 No Content`

**Errors**:

- `401 Unauthorized` - likerId doesn't match authenticated user
- `404 Not Found` - Plugin doesn't exist
- `409 Conflict` - Already liked (when liking) or not liked (when unliking)
- `500 Internal Server Error` - Race condition detected

**Key File**: `PluginController.cs:118-138`

---

### Get Plugin Versions

```http
GET /api/v1/plugin/slug/{username}/{pluginName}/versions?skip=0&limit=50&search=query
```

**Description**: List plugin versions.

**Path Parameters**:

| Parameter    | Type   | Description |
| ------------ | ------ | ----------- |
| `username`   | string | Username    |
| `pluginName` | string | Plugin name |

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
    "dockerImage": "my-registry/plugin",
    "dockerTag": "v1",
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

**Key File**: `PluginController.cs:150-162`

---

### Get Plugin Versions by ID

```http
GET /api/v1/plugin/id/{userId}/{pluginId:guid}/versions?skip=0&limit=50&search=query
```

**Description**: List plugin versions by plugin ID.

**Path Parameters**:

| Parameter  | Type   | Description |
| ---------- | ------ | ----------- |
| `userId`   | string | User ID     |
| `pluginId` | Guid   | Plugin ID   |

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
    "dockerImage": "my-registry/plugin",
    "dockerTag": "v1",
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

**Key File**: `PluginController.cs:164-176`

---

### Get Plugin Version by Number

```http
GET /api/v1/plugin/slug/{username}/{pluginName}/versions/{ver}?bumpDownload=true
```

**Description**: Get a specific plugin version.

**Path Parameters**:

| Parameter    | Type   | Description    |
| ------------ | ------ | -------------- |
| `username`   | string | Username       |
| `pluginName` | string | Plugin name    |
| `ver`        | ulong  | Version number |

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
  "dockerImage": "my-registry/plugin",
  "dockerTag": "v1",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Key File**: `PluginController.cs:178-197`

---

### Get Latest Plugin Version

```http
GET /api/v1/plugin/slug/{username}/{pluginName}/versions/latest?bumpDownload=true
```

**Description**: Get the latest plugin version.

**Path Parameters**:

| Parameter    | Type   | Description |
| ------------ | ------ | ----------- |
| `username`   | string | Username    |
| `pluginName` | string | Plugin name |

**Query Parameters**:

| Parameter      | Type    | Default | Description              |
| -------------- | ------- | ------- | ------------------------ |
| `bumpDownload` | boolean | false   | Increment download count |

**Response**: `200 OK` (same as Get Plugin Version by Number)

**Key File**: `PluginController.cs:199-213`

---

### Get Plugin Version by ID and Number

```http
GET /api/v1/plugin/id/{userId}/{pluginId:guid}/versions/{ver}
```

**Description**: Get a specific plugin version by plugin ID.

**Path Parameters**:

| Parameter  | Type   | Description    |
| ---------- | ------ | -------------- |
| `userId`   | string | User ID        |
| `pluginId` | Guid   | Plugin ID      |
| `ver`      | ulong  | Version number |

**Response**: `200 OK`

```json
{
  "id": "version-guid",
  "version": 1,
  "description": "Initial version",
  "dockerImage": "my-registry/plugin",
  "dockerTag": "v1",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Key File**: `PluginController.cs:215-229`

---

### Get Plugin Version by ID

```http
GET /api/v1/plugin/versions/{versionId:guid}
```

**Description**: Get a plugin version by its version ID.

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
  "dockerImage": "my-registry/plugin",
  "dockerTag": "v1",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Key File**: `PluginController.cs:357-365`

---

### Create Plugin Version

```http
POST /api/v1/plugin/slug/{username}/{pluginName}/versions
```

**Description**: Create a new plugin version.

**Path Parameters**:

| Parameter    | Type   | Description |
| ------------ | ------ | ----------- |
| `username`   | string | Username    |
| `pluginName` | string | Plugin name |

**Request Body**:

```json
{
  "description": "New version",
  "dockerImage": "my-registry/plugin",
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
- `404 Not Found` - Plugin not found

**Key File**: `PluginController.cs:231-262`

---

### Create Plugin Version by ID

```http
POST /api/v1/plugin/id/{userId}/{pluginId:guid}/versions
```

**Description**: Create a new plugin version by plugin ID.

**Path Parameters**:

| Parameter  | Type   | Description                             |
| ---------- | ------ | --------------------------------------- |
| `userId`   | string | User ID (must match authenticated user) |
| `pluginId` | Guid   | Plugin ID                               |

**Request Body**:

```json
{
  "description": "New version",
  "dockerImage": "my-registry/plugin",
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
- `404 Not Found` - Plugin not found

**Key File**: `PluginController.cs:264-291`

---

### Update Plugin Version

```http
PUT /api/v1/plugin/id/{userId}/{pluginId:guid}/versions/{ver}
```

**Description**: Update a plugin version metadata.

**Path Parameters**:

| Parameter  | Type   | Description                             |
| ---------- | ------ | --------------------------------------- |
| `userId`   | string | User ID (must match authenticated user) |
| `pluginId` | Guid   | Plugin ID                               |
| `ver`      | ulong  | Version number                          |

**Request Body**:

```json
{
  "description": "Updated version description",
  "dockerImage": "my-registry/plugin",
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
- `404 Not Found` - Plugin doesn't exist

**Key File**: `PluginController.cs:293-319`

---

### Push Plugin

```http
POST /api/v1/plugin/push/{username}
```

**Description**: Create or update a plugin and create a new version in a single operation.

**Path Parameters**:

| Parameter  | Type   | Description                              |
| ---------- | ------ | ---------------------------------------- |
| `username` | string | Username (must match authenticated user) |

**Request Body**:

```json
{
  "name": "notification-plugin",
  "project": "my-project",
  "source": "github.com/user/plugin",
  "email": "user@example.com",
  "description": "Notification plugin",
  "tags": ["notification", "plugin"],
  "readme": "# Usage\n...",
  "version": {
    "description": "Initial version",
    "dockerImage": "my-registry/plugin",
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
- `404 Not Found` - Plugin not found

**Key File**: `PluginController.cs:321-355`

---

## Related

- [Plugin Registry Feature](../../features/05-plugin-registry.md)
- [Plugin Repository](../../../../App/Modules/Cyan/Data/Repositories/PluginRepository.cs)
