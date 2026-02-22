# Authorization Concept

**What**: Scope-based authorization using HasAny/HasAll policy handlers.
**Why**: Enables flexible, claims-based access control for different user roles.

## Policy Types

Zinc supports two authorization policy types:

### HasAny Policy

User must have **at least one** of the required scopes/roles.

**Key File**: `App/StartUp/Services/Auth/HasAnyHandler.cs:5-26`

### HasAll Policy

User must have **all** of the required scopes/roles.

**Key File**: `App/StartUp/Services/Auth/HasAllHandler.cs:5-33`

## Policy Configuration

Policies are configured in `appsettings.json`:

```json
{
  "Auth": {
    "Settings": {
      "Policies": {
        "AdminOnly": {
          "Type": "All",
          "Field": "roles",
          "Target": ["admin"]
        },
        "ReadAccess": {
          "Type": "Any",
          "Field": "scope",
          "Target": ["read:templates", "read:all"]
        }
      }
    }
  }
}
```

**Key File**: `App/StartUp/Services/AuthService.cs:88-112`

## Claim Field Mapping

| Field   | Source                                                         | Format                  | Handler                 |
| ------- | -------------------------------------------------------------- | ----------------------- | ----------------------- |
| `scope` | JWT claim                                                      | Space-separated strings | Split before comparison |
| `roles` | `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` | Individual claims       | Direct comparison       |
| Other   | Any claim type                                                 | Individual claims       | Direct comparison       |

## Usage in Controllers

Policies are applied using attributes:

```csharp
[Authorize(Policy = "AdminOnly")]
public async Task<ActionResult> AdminEndpoint() { }

[Authorize(Policy = "ReadAccess")]
public async Task<ActionResult> ReadEndpoint() { }
```

## Authorization vs Authentication

| Aspect            | Authentication      | Authorization        |
| ----------------- | ------------------- | -------------------- |
| **Question**      | Who are you?        | What can you do?     |
| **Mechanism**     | JWT or API Key      | Policy handlers      |
| **Claims Source** | Descope or Database | Policy configuration |
| **Failure**       | 401 Unauthorized    | 403 Forbidden        |

## Related

- [Authorization Feature](../features/02-authorization.md) - Implementation details, flows, and edge cases
- [Authentication Concept](./01-authentication.md) - How users are authenticated
