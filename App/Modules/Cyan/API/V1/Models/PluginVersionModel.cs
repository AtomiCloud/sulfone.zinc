namespace App.Modules.Cyan.API.V1.Models;

public record SearchPluginVersionQuery(string? Search, int? Limit, int? Skip);

public record CreatePluginVersionReq(
  string Description, string DockerReference, string DockerSha);

public record UpdatePluginVersionReq(string Description);

public record PluginVersionPrincipalResp(
  Guid Id, ulong Version, DateTime CreatedAt,
  string Description, string DockerReference, string DockerSha);

public record PluginVersionResp(
  PluginVersionPrincipalResp Principal,
  PluginPrincipalResp Plugin);