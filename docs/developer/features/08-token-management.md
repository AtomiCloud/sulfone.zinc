# Token Management Feature

**What**: API token CRUD operations with validation and revocation.
**Why**: Enables service-to-service authentication via API keys.

**Key Files**:

- `Domain/Service/TokenService.cs` → `Create()`, `Validate()`, `Revoke()`, `Delete()`
- `App/Modules/Users/Data/TokenData.cs` → Token data model
- `App/Modules/Users/API/V1/UserController.cs` → Token endpoints

## Overview

The Token Management feature allows users to create, view, and revoke API tokens for service-to-service authentication. Tokens are 64-character random strings stored in plaintext with a revocation flag.

## Token Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Active: Create Token
    Active --> Active: Validate - returns User
    Active --> Revoked: Revoke
    Revoked --> [*]: Delete - optional

    note right of Active
        Token can be used
        for authentication
    end note

    note right of Revoked
        Token cannot be used
        but record kept
    end note
```

## Token Model

```csharp
public record TokenData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public bool Revoked { get; set; } = false;
    public string UserId { get; set; } = string.Empty;
    public UserData User { get; set; } = new();
}
```

**Key File**: `App/Modules/Users/Data/TokenData.cs`

## Operations

| Operation | Endpoint                                        | Purpose            |
| --------- | ----------------------------------------------- | ------------------ |
| Search    | `GET /api/v1/user/{id}/token`                   | List user's tokens |
| Get       | `GET /api/v1/user/{id}/token/{tokenId}`         | Get specific token |
| Create    | `POST /api/v1/user/{id}/token`                  | Create new token   |
| Update    | `PUT /api/v1/user/{id}/token/{tokenId}`         | Update token name  |
| Revoke    | `POST /api/v1/user/{id}/token/{tokenId}/revoke` | Revoke token       |
| Delete    | `DELETE /api/v1/user/{id}/token/{tokenId}`      | Delete token       |

## Flow

### Create Token Sequence

```mermaid
sequenceDiagram
    participant Client
    participant Controller as UserController
    participant Validator as CreateTokenReqValidator
    participant TokenSvc as TokenService
    participant Generator as ApiKeyGenerator
    participant Repository as TokenRepository
    participant DB as Database

    Client->>Controller: POST /api/v1/user/id/{id}/token
    Controller->>Controller: Check userId == sub
    Controller->>Validator: ValidateAsync
    Validator-->>Controller: ValidationResult

    alt Validation Fails
        Controller-->>Client: 400 Bad Request
    else Validation Passes
        Controller->>TokenSvc: Create
        TokenSvc->>Generator: Generate
        Generator-->>TokenSvc: 64-char token
        TokenSvc->>Repository: Create
        Repository->>DB: INSERT INTO Tokens
        DB-->>Repository: TokenData
        Repository-->>TokenSvc: TokenPrincipal
        TokenSvc-->>Controller: Result
        Controller-->>Client: 201 Created
    end
```

**Key File**: `Domain/Service/TokenService.cs:19-24`

### Validate Token Sequence

```mermaid
sequenceDiagram
    participant Handler as ApiKeyAuthenticationHandler
    participant TokenSvc as TokenService
    participant Repository as TokenRepository
    participant DB as Database

    Handler->>TokenSvc: Validate(token)
    TokenSvc->>Repository: Validate(token)
    Repository->>DB: SELECT FROM Tokens WHERE ApiToken = token
    DB-->>Repository: TokenData

    alt Token not found or revoked
        Repository-->>TokenSvc: null
        TokenSvc-->>Handler: null
        Handler-->>Handler: Return Fail
    else Token active
        Repository->>DB: INCLUDE User
        DB-->>Repository: TokenData with User
        Repository-->>TokenSvc: UserPrincipal
        TokenSvc-->>Handler: UserPrincipal
        Handler-->>Handler: Create Claims
    end
```

**Key File**: `Domain/Service/TokenService.cs:36-39`

### Revoke Token Flow

```mermaid
flowchart TB
    Start["POST /revoke endpoint"] --> CheckUser{User matches?}
    CheckUser -->|No| Unauthorized[401 Unauthorized]
    CheckUser -->|Yes| Fetch[Fetch Token]
    Fetch --> Found{Exists?}
    Found -->|No| NotFound[404 Not Found]
    Found -->|Yes| SetRevoked[Set Revoked = true]
    SetRevoked --> Save[Save to Database]
    Save --> Success[Return Unit]
```

**Key File**: `Domain/Service/TokenService.cs:31-34`

## Token Generation

Tokens are generated using the `PasswordGenerator` library:

```csharp
public string Generate()
{
    var pwd = new Password()
        .IncludeLowercase()
        .IncludeNumeric()
        .IncludeUppercase()
        .LengthRequired(64);
    return pwd.Next();
}
```

**Key File**: `Domain/Service/ApiKeyGenerator.cs:7-15`

## Edge Cases

| Case                         | Behavior                      | Key File                              |
| ---------------------------- | ----------------------------- | ------------------------------------- |
| Validate non-existent token  | Returns null                  | `TokenService.cs:36-39`               |
| Validate revoked token       | Returns null                  | Repository filters `Revoked == false` |
| Create duplicate name        | Database constraint violation | `MainDbContext.cs`                    |
| Revoke already revoked token | No-op (idempotent)            | `TokenService.cs:31-34`               |

## Security Considerations

- **Plaintext Storage**: Tokens are stored in plaintext (NOT hashed)
- **Single Use**: Token is shown only on creation
- **Revocation**: Soft delete (sets `Revoked = true`)
- **User Ownership**: Users can only manage their own tokens

## Related

- [Authentication Concept](../concepts/01-authentication.md) - How tokens are used
- [Authentication Feature](./01-authentication.md) - Token validation in auth flow
- [User Module](../modules/02-users.md) - User and token data models
- [User API](../surfaces/api/04-user.md) - Token endpoints
