using App.Modules.Users.API.V1;

namespace App.Modules.Cyan.API.V1.Models;

public record SearchPluginQuery(string? Owner, string? Search, int? Limit, int? Skip);

public record CreatePluginReq(string Name, string Project, string Source,
  string Email, string[] Tags, string Description, string Readme);

public record UpdatePluginReq(string Project, string Source,
  string Email, string[] Tags, string Description, string Readme);

public record PluginPrincipalResp(
    Guid Id, string Name, string Project, string Source,
    string Email, string[] Tags, string Description, string Readme, string UserId);

public record PluginInfoResp(
  uint Downloads, uint Dependencies, uint Stars);

public record PluginResp(
    PluginPrincipalResp Principal, PluginInfoResp Info,
    UserPrincipalResp User,
    IEnumerable<PluginVersionPrincipalResp> Versions);


