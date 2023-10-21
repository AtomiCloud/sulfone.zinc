using Domain.Model;

namespace Domain;

public record Template
{
  public required TemplatePrincipal Principal { get; init; }
  public required UserPrincipal User { get; init; }
}

public record TemplatePrincipal
{
  public required Guid Id { get; init; }
  public required string Name { get; init; }
  public required uint Version { get; init; }
  public required TemplateRecord Record { get; init; }
}

public record TemplateRecord
{
  public required string BlobPath { get; init; }

  public required string DockerReference { get; init; }

  public required string DockerSha { get; init; }
}
