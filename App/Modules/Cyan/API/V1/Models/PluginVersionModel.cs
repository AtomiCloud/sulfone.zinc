namespace App.Modules.Cyan.API.V1.Models;

public record SearchPluginVersionQuery(string? Search, int? Limit, int? Skip);

public record CreatePluginVersionReq(
  string Description, string DockerReference, string DockerTag);

public record UpdatePluginVersionReq(string Description);

public record PushPluginReq(
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

public record PluginVersionPrincipalResp(
  Guid Id, ulong Version, DateTime CreatedAt,
  string Description, string DockerReference, string DockerTag);

public record PluginVersionResp(
  PluginVersionPrincipalResp Principal,
  PluginPrincipalResp Plugin);
