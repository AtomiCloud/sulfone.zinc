# Getting Started with Zinc

This guide will help you set up and run Zinc locally.

## Prerequisites

- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Direnv** - Environment management
- **Tilt** - Development environment orchestration

## Development Setup

Zinc is developed using Tilt and Lapras for local development. The database and other infrastructure are managed automatically.

### 1. Clone and Enter the Repository

```bash
git clone <repo-url>
cd zinc
```

Direnv will automatically load the development environment when you enter the directory.

### 2. Start Development Environment

```bash
pls dev
```

This uses Tilt to spin up the full development stack including the database.

### 3. Configure Authentication (Optional)

Authentication is configured via environment variables or a configuration file:

**Environment Variables:**

- `Auth__Enabled` - Enable JWT authentication (default: false)
- `Auth__Settings__Domain` - Descope domain
- `Auth__Settings__Audience` - Expected JWT audience
- `Auth__Settings__Issuer` - Expected JWT issuer

**Key File**: `App/StartUp/Options/Auth/AuthOption.cs`

## Common Tasks

All tasks are defined in the Taskfile. Use `pls` to run them:

| Command     | Description                             |
| ----------- | --------------------------------------- |
| `pls dev`   | Start development environment with Tilt |
| `pls build` | Build the application                   |
| `pls test`  | Run tests                               |
| `pls lint`  | Run linters                             |

Run `pls --list` to see all available tasks.

## Verification

Test that the service is running:

<!--
NOTE: Port 5000 is the ASP.NET Core default and is used here for simplicity in a getting-started guide.
For production or custom configurations, refer to the Configuration Reference table below or your Tilt/Lapras config.
-->

```bash
curl http://localhost:5000/
```

Expected response:

```json
{
  "landscape": "...",
  "platform": "...",
  "service": "...",
  "module": "...",
  "version": "...",
  "status": "OK",
  "timeStamp": "2024-..."
}
```

## Next Steps

- Read [Architecture Overview](./02-architecture.md)
- Explore [Core Concepts](./concepts/)
- Review [API Endpoints](./surfaces/api/)

## Configuration Reference

| Key                         | Type   | Default       | Description                  |
| --------------------------- | ------ | ------------- | ---------------------------- |
| `Auth.Enabled`              | bool   | `false`       | Enable JWT authentication    |
| `Auth.Settings.Domain`      | string | -             | Descope domain               |
| `Auth.Settings.Audience`    | string | -             | Expected JWT audience        |
| `Auth.Settings.Issuer`      | string | -             | Expected JWT issuer          |
| `ConnectionStrings.Default` | string | -             | PostgreSQL connection string |
| `Logging.LogLevel.Default`  | string | `Information` | Log verbosity                |
