namespace App.Modules.Cyan.API.V1.Models;

public record SearchProcessorVersionQuery(string? Search, int? Limit, int? Skip);

public record CreateProcessorVersionReq(
  string Description, string DockerReference, string DockerSha);

public record UpdateProcessorVersionReq(string Description);

public record ProcessorVersionPrincipalResp(
  Guid Id, ulong Version, DateTime CreatedAt,
  string Description, string DockerReference, string DockerSha);

public record ProcessorVersionResp(
  ProcessorVersionPrincipalResp Principal,
  ProcessorPrincipalResp Processor);
