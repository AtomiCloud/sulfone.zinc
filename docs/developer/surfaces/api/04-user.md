# User API

**Base Path**: `/api/v1/user`
**Key File**: `App/Modules/Users/API/V1/UserController.cs`

## Endpoints

### Search Users (Admin Only)

```http
GET /api/v1/user?skip=0&limit=50&search=query
```

**Description**: Search and list users. Requires admin role.

**Authorization**: `Policy = "AdminOnly"`

**Query Parameters**:

| Parameter | Type   | Required | Description                  |
| --------- | ------ | -------- | ---------------------------- |
| `skip`    | int    | No       | Results to skip (default: 0) |
| `limit`   | int    | No       | Max results (default: 50)    |
| `search`  | string | No       | Filter by username           |

**Response**: `200 OK`

```json
[
  {
    "id": "user-id-123",
    "username": "alice",
    "description": "CI/CD engineer",
    "tags": ["ci", "devops"],
    "email": "alice@example.com",
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

**Error**: `403 Forbidden` if not admin

**Key File**: `UserController.cs:33-43`

---

### Get Current User

```http
GET /api/v1/user/Me
```

**Description**: Get the authenticated user's ID.

**Authorization**: Required

**Response**: `200 OK` (plain text)

```text
user-id-123
```

**Key File**: `UserController.cs:45-49`

---

### Get User by ID

<!--
NOTE: The {id} parameter uses string type without :guid constraint in GET and PUT routes.
This matches the actual controller implementation. DELETE uses {id:guid} constraint.
This inconsistency reflects the current code behavior - changing would require code modifications.
-->

```http
GET /api/v1/user/{id}
```

**Description**: Get a user by ID. Only the user themselves can access their data.

**Path Parameters**:

| Parameter | Type   | Description |
| --------- | ------ | ----------- |
| `id`      | string | User ID     |

**Response**: `200 OK`

```json
{
  "id": "user-id-123",
  "username": "alice",
  "description": "CI/CD engineer",
  "tags": ["ci", "devops"],
  "email": "alice@example.com",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Errors**:

- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Trying to access another user's data
- `404 Not Found` - User doesn't exist

**Key File**: `UserController.cs:51-72`

---

### Get User by Username

```http
GET /api/v1/user/username/{username}
```

**Description**: Get a user by username. Only the user themselves can access their data.

**Path Parameters**:

| Parameter  | Type   | Description |
| ---------- | ------ | ----------- |
| `username` | string | Username    |

**Response**: `200 OK` (same as Get by ID)

**Errors**: Same as Get by ID

**Key File**: `UserController.cs:74-90`

---

### Check User Exists

```http
GET /api/v1/user/exist/{username}
```

**Description**: Check if a username exists. Any authenticated user can check.

**Path Parameters**:

| Parameter  | Type   | Description       |
| ---------- | ------ | ----------------- |
| `username` | string | Username to check |

**Response**: `200 OK`

```json
{
  "exists": true
}
```

**Key File**: `UserController.cs:92-97`

---

### Create User

```http
POST /api/v1/user
```

**Description**: Create a new user.

<!--
NOTE: "Authorization: None" means no additional authorization policy is enforced beyond token validation.
A valid Descope-issued JWT must be present in the Authorization header for self-registration.
The user identity is extracted from the JWT to create the user account.
-->

**Authorization**: None (requires valid Descope JWT in Authorization header for self-registration)

**Request Body**:

```json
{
  "username": "alice",
  "description": "CI/CD engineer",
  "tags": ["ci", "devops"],
  "email": "alice@example.com"
}
```

**Response**: `201 Created`

```json
{
  "id": "user-id-123",
  "username": "alice",
  "description": "CI/CD engineer",
  "tags": ["ci", "devops"],
  "email": "alice@example.com",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Errors**:

- `400 Bad Request` - Validation failed
- `409 Conflict` - Username already exists

**Key File**: `UserController.cs:99-119`

---

### Update User

```http
PUT /api/v1/user/{id}
```

**Description**: Update user metadata. Only the user themselves can update their data.

**Path Parameters**:

| Parameter | Type   | Description |
| --------- | ------ | ----------- |
| `id`      | string | User ID     |

**Request Body**:

```json
{
  "description": "Updated description",
  "tags": ["ci", "devops", "updated"],
  "email": "newemail@example.com"
}
```

**Response**: `200 OK`

```json
{
  "id": "user-id-123",
  "username": "alice",
  "description": "Updated description",
  "tags": ["ci", "devops", "updated"],
  "email": "newemail@example.com",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Errors**:

- `400 Bad Request` - Validation failed
- `403 Forbidden` - Trying to update another user's data
- `404 Not Found` - User doesn't exist

**Key File**: `UserController.cs:120-138`

---

### Delete User

```http
DELETE /api/v1/user/{id:guid}
```

**Description**: Delete a user. Requires Admin scope.

**Path Parameters**:

| Parameter | Type | Description           |
| --------- | ---- | --------------------- |
| `id`      | Guid | User ID (GUID format) |

**Authorization**: Requires `Admin` scope (via `AuthPolicies.OnlyAdmin` policy)

**Response**: `204 No Content`

**Errors**:

- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User lacks Admin scope
- `404 Not Found` - User doesn't exist

**Key File**: `UserController.cs:140-148`

---

### Get User Tokens

```http
GET /api/v1/user/{userId}/tokens
```

**Description**: List all API tokens for a user. Only the user themselves can access their tokens.

**Path Parameters**:

| Parameter | Type   | Description |
| --------- | ------ | ----------- |
| `userId`  | string | User ID     |

**Response**: `200 OK`

```json
[
  {
    "id": "token-guid-1",
    "name": "CLI Token",
    "revoked": false,
    "createdAt": "2024-01-01T00:00:00Z"
  },
  {
    "id": "token-guid-2",
    "name": "CI Token",
    "revoked": false,
    "createdAt": "2024-01-02T00:00:00Z"
  }
]
```

**Errors**:

- `403 Forbidden` - Trying to access another user's tokens

**Key File**: `UserController.cs:150-164`

---

### Create Token

```http
POST /api/v1/user/{userId}/tokens
```

**Description**: Create a new API token. Only the user themselves can create tokens.

**Path Parameters**:

| Parameter | Type   | Description |
| --------- | ------ | ----------- |
| `userId`  | string | User ID     |

**Request Body**:

```json
{
  "name": "My CLI Token"
}
```

**Response**: `201 Created`

```json
{
  "id": "new-token-guid",
  "name": "My CLI Token",
  "token": "XXX",
  "revoked": false,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Important**: The `token` field is only returned on creation. Save it securely.

**Errors**:

- `400 Bad Request` - Validation failed
- `403 Forbidden` - Trying to create token for another user

**Key File**: `UserController.cs:166-186`

---

### Update Token

```http
PUT /api/v1/user/{userId}/tokens/{tokenId:guid}
```

**Description**: Update token name. Only the token owner can update.

**Path Parameters**:

| Parameter | Type   | Description |
| --------- | ------ | ----------- |
| `userId`  | string | User ID     |
| `tokenId` | Guid   | Token ID    |

**Request Body**:

```json
{
  "name": "Updated Token Name"
}
```

**Response**: `200 OK`

```json
{
  "id": "token-guid",
  "name": "Updated Token Name",
  "revoked": false,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Errors**:

- `400 Bad Request` - Validation failed
- `403 Forbidden` - Trying to update another user's token
- `404 Not Found` - Token doesn't exist

**Key File**: `UserController.cs:188-217`

---

### Revoke Token

```http
POST /api/v1/user/{userId}/tokens/{tokenId:guid}/revoke
```

**Description**: Revoke an API token. Only the token owner can revoke.

**Path Parameters**:

| Parameter | Type   | Description |
| --------- | ------ | ----------- |
| `userId`  | string | User ID     |
| `tokenId` | Guid   | Token ID    |

**Response**: `204 No Content`

**Errors**:

- `403 Forbidden` - Trying to revoke another user's token
- `404 Not Found` - Token doesn't exist

**Key File**: `UserController.cs:219-240`

---

### Delete Token

```http
DELETE /api/v1/user/{userId}/tokens/{tokenId:guid}
```

**Description**: Permanently delete an API token. Only the token owner can delete.

**Path Parameters**:

| Parameter | Type   | Description |
| --------- | ------ | ----------- |
| `userId`  | string | User ID     |
| `tokenId` | Guid   | Token ID    |

**Response**: `204 No Content`

**Errors**:

- `403 Forbidden` - Trying to delete another user's token
- `404 Not Found` - Token doesn't exist

**Key File**: `UserController.cs:242-263`

---

## Related

- [Token Management Feature](../../features/08-token-management.md)
- [Authentication Feature](../../features/01-authentication.md)
- [User Module](../../modules/03-users.md)
