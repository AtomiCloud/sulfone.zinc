namespace App.Modules.Cyan.API.V1.Models;

public record SearchResolverVersionQuery(string? Search, int? Limit, int? Skip);

public record CreateResolverVersionReq(
  string Description,
  string DockerReference,
  string DockerTag
);

public record UpdateResolverVersionReq(string Description);

public record PushResolverReq(
  string Name,
  string Project,
  string Source,
  string Email,
  string[] Tags,
  string Description,
  string Readme,
  string VersionDescription,
  string DockerReference,
  string DockerTag
);

public record ResolverVersionPrincipalResp(
  Guid Id,
  ulong Version,
  DateTime CreatedAt,
  string Description,
  string DockerReference,
  string DockerTag
);

public record ResolverVersionResp(
  ResolverVersionPrincipalResp Principal,
  ResolverPrincipalResp Resolver
);
