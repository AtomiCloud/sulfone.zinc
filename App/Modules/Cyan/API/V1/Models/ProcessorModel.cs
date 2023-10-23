using App.Modules.Users.API.V1;

namespace App.Modules.Cyan.API.V1.Models;

public record SearchProcessorQuery(string? Owner, string? Search, int? Limit, int? Skip);

public record CreateProcessorReq(string Name, string Project, string Source,
  string Email, string[] Tags, string Description, string Readme);

public record UpdateProcessorReq(string Project, string Source,
  string Email, string[] Tags, string Description, string Readme);

public record ProcessorPrincipalResp(
    Guid Id, string Name, string Project, string Source,
    string Email, string[] Tags, string Description, string Readme);

public record ProcessorInfoResp(
  uint Downloads, uint Dependencies, uint Stars);

public record ProcessorResp(
    ProcessorPrincipalResp Principal, ProcessorInfoResp Info,
    UserPrincipalResp User,
    IEnumerable<ProcessorVersionPrincipalResp> Versions);


