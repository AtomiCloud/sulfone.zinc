using App.Modules.Users.API.V1;

namespace App.Modules.Cyan.API.V1.Models;

public record SearchTemplateQuery(string? Owner, string? Search, int? Limit, int? Skip);

public record CreateTemplateReq(string Name, string Project, string Source,
  string Email, string[] Tags, string Description, string Readme);

public record UpdateTemplateReq(string Project, string Source,
  string Email, string[] Tags, string Description, string Readme);

public record TemplatePrincipalResp(
    Guid Id, string Name, string Project, string Source,
    string Email, string[] Tags, string Description, string Readme, string UserId);

public record TemplateInfoResp(
  uint Downloads, uint Stars);

public record TemplateResp(
    TemplatePrincipalResp Principal, TemplateInfoResp Info,
    UserPrincipalResp User,
    IEnumerable<TemplateVersionPrincipalResp> Versions);


