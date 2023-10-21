namespace Domain.Model;

public record TemplateSearch
{
}

public record Template
{
  public required TemplatePrincipal Principal { get; init; }
  public required UserPrincipal User { get; init; }
}

public record TemplatePrincipal
{
  public required Guid Id { get; init; }

  // User Controlled, only on create
  public required TemplateProperty Prop { get; init; }

  // Telemetry, non-user controlled
  public required TemplateInfo Info { get; init; }

  // User Controlled, updatable, metadata
  public required TemplateMetadata Metadata { get; init; }
}

public record TemplateInfo
{
  public required int Likes { get; init; }
  public required int Downloads { get; init; }
}

public record TemplateMetadata
{
  public required string Project { get; init; }
  public required string Source { get; init; }
  public required string Email { get; init; }
  public required string Tags { get; init; }
  public required string Description { get; init; }
  public required string Readme { get; init; }
}

public record TemplateProperty
{
  public required string Name { get; init; }

  public required uint Version { get; init; }

  public required string BlobDockerReference { get; init; }

  public required string BlobDockerSha { get; init; }

  public required string TemplateDockerReference { get; init; }

  public required string TemplateDockerSha { get; init; }
}
