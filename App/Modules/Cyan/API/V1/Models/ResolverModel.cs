using App.Modules.Users.API.V1;

namespace App.Modules.Cyan.API.V1.Models;

public record SearchResolverQuery(string? Owner, string? Search, int? Limit, int? Skip);

public record CreateResolverReq(
  string Name,
  string Project,
  string Source,
  string Email,
  string[] Tags,
  string Description,
  string Readme
);

public record UpdateResolverReq(
  string Project,
  string Source,
  string Email,
  string[] Tags,
  string Description,
  string Readme
);

public record ResolverPrincipalResp(
  Guid Id,
  string Name,
  string Project,
  string Source,
  string Email,
  string[] Tags,
  string Description,
  string Readme,
  string UserId
);

public record ResolverInfoResp(uint Downloads, uint Dependencies, uint Stars);

public record ResolverResp(
  ResolverPrincipalResp Principal,
  ResolverInfoResp Info,
  UserPrincipalResp User,
  IEnumerable<ResolverVersionPrincipalResp> Versions
);
