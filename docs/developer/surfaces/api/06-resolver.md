# Resolver API

<!--
NOTE: Resolver controllers follow the same pattern
as Template and Processor controllers. They are located at
App/Modules/Cyan/API/V1/Controllers/ResolverController.cs.
This document uses the same versioning pattern.
-->

**Base Path**: `/api/v1/resolver`

**Key File**: `App/Modules/Cyan/API/V1/Controllers/ResolverController.cs`

## Endpoints

### Search Resolvers

```http
GET /
```

**Description**: Search resolvers with optional filters.

**Authorization**: Not required

**Query Parameters**:

| Name     | Type   | Required | Description               |
| -------- | ------ | -------- | ------------------------- |
| `Owner`  | string | no       | Filter by owner username  |
| `Search` | string | no       | Full-text search query    |
| `Limit`  | int    | no       | Max results (default: 20) |
| `Skip`   | int    | no       | Skip results (default: 0) |

**Response**: `200 OK`

```json
[
  {
    "id": "guid",
    "name": "my-resolver",
    "project": "https://github.com/user/project",
    "source": "https://github.com/user/project",
    "email": "user@example.com",
    "tags": ["tag1", "tag2"],
    "description": "Resolver description",
    "readme": "# Resolver\n...",
    "userId": "user-id"
  }
]
```

---

### Get Resolver by ID

```http
GET /id/{userId}/{resolverId:guid}
```

**Description**: Get resolver by user ID and resolver ID.

**Authorization**: Not required

**Response**: `200 OK`

```json
{
  "principal": { ... },
  "info": {
    "downloads": 0,
    "dependencies": 0,
    "stars": 0
  },
  "user": { ... },
  "versions": [ ... ]
}
```

---

### Get Resolver by Slug

```http
GET /slug/{username}/{name}
```

**Description**: Get resolver by owner username and resolver name.

**Authorization**: Not required

**Response**: `200 OK`

```json
{
  "principal": { ... },
  "info": { ... },
  "user": { ... },
  "versions": [ ... ]
}
```

---

### Create Resolver

```http
POST /id/{userId}
```

**Description**: Create a new resolver.

**Authorization**: Required (owner only)

**Request Body**:

```json
{
  "name": "my-resolver",
  "project": "https://github.com/user/project",
  "source": "https://github.com/user/project",
  "email": "user@example.com",
  "tags": ["tag1", "tag2"],
  "description": "Resolver description",
  "readme": "# Resolver\n..."
}
```

**Response**: `201 Created`

---

### Update Resolver

```http
PUT /id/{userId}/{resolverId}
```

**Description**: Update resolver metadata.

**Authorization**: Required (owner only)

**Note**: Only metadata fields (project, source, email, tags, description, readme) can be updated. Docker references are set during version creation only.

**Request Body**:

```json
{
  "project": "https://github.com/user/project",
  "source": "https://github.com/user/project",
  "email": "user@example.com",
  "tags": ["tag1", "tag2"],
  "description": "Updated description",
  "readme": "# Updated Resolver\n..."
}
```

**Response**: `200 OK`

---

### Delete Resolver

```http
DELETE /id/{userId}/{resolverId:guid}
```

**Description**: Delete a resolver.

**Authorization**: Required (admin only)

**Response**: `204 No Content`

---

### Like/Unlike Resolver

```http
POST /slug/{username}/{resolverName}/like/{likerId}/{like}
```

**Description**: Like or unlike a resolver.

**Authorization**: Required (liker only)

**Parameters**:
| Name | Type | Description |
| ------ | ------ | ------------------------------ |
| `like` | bool | `true` to like, `false` to unlike |

**Response**: `200 OK`

---

### List Resolver Versions

```http
GET /slug/{username}/{resolverName}/versions
```

**Description**: List all versions of a resolver.

**Authorization**: Not required

**Response**: `200 OK`

---

### Get Latest Resolver Version

```http
GET /slug/{username}/{resolverName}/versions/latest
```

**Description**: Get the latest version of a resolver.

**Authorization**: Not required

**Query Parameters**:
| Name | Type | Required | Description |
| ------------- | ---- | -------- | --------------------------------- |
| `bumpDownload` | bool | no | Increment download count (default: false) |

**Response**: `200 OK`

---

### Get Specific Resolver Version

```http
GET /slug/{username}/{resolverName}/versions/{ver}
```

**Description**: Get a specific version of a resolver.

**Authorization**: Not required

**Query Parameters**:
| Name | Type | Required | Description |
| ------------- | ---- | -------- | --------------------------------- |
| `bumpDownload` | bool | no | Increment download count (default: false) |

**Response**: `200 OK`

---

### Create Resolver Version

```http
POST /slug/{username}/{resolverName}/versions
```

**Description**: Create a new version of a resolver.

**Authorization**: Required (owner only)

**Request Body**:

```json
{
  "description": "Version description",
  "dockerReference": "docker.io/user/resolver",
  "dockerTag": "latest"
}
```

**Response**: `201 Created`

---

### Update Resolver Version

```http
PUT /id/{userId}/{resolverId}/versions/{ver}
```

**Description**: Update version metadata.

**Authorization**: Required (owner only)

**Note**: Only description can be updated. Docker reference and tag are immutable after creation.

**Request Body**:

```json
{
  "description": "Updated description"
}
```

**Response**: `200 OK`

---

### Push Resolver (Upsert)

```http
POST /push/{username}
```

**Description**: Create or update a resolver and create a new version in one operation.

**Authorization**: Required (owner only)

**Request Body**:

```json
{
  "name": "my-resolver",
  "project": "https://github.com/user/project",
  "source": "https://github.com/user/project",
  "email": "user@example.com",
  "tags": ["tag1", "tag2"],
  "description": "Resolver description",
  "readme": "# Resolver\n...",
  "versionDescription": "Version description",
  "dockerReference": "docker.io/user/resolver",
  "dockerTag": "latest"
}
```

**Response**: `201 Created`

---

### Get Version by ID

```http
GET /versions/{versionId:guid}
```

**Description**: Get a resolver version by its ID.

**Authorization**: Not required

**Response**: `200 OK`

---

## Error Responses

| Code | Type            | Description                              |
| ---- | --------------- | ---------------------------------------- |
| 400  | Invalid Request | Invalid request body or parameters       |
| 401  | Unauthorized    | Authentication required or failed        |
| 403  | Forbidden       | Admin access required (delete operation) |
| 404  | Not Found       | Resolver not found                       |
| 409  | Conflict        | Duplicate name or version conflict       |

## Related

- [Resolver Registry Feature](../../features/09-resolver-registry.md)
- [Registry Concept](../../concepts/03-registry.md)
- [Version Concept](../../concepts/04-version.md)
- [Dependency Concept](../../concepts/05-dependency.md)
