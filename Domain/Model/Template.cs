namespace Domain.Model;

public record TemplateSearch
{
  public string? Owner { get; init; }
  public string? Search { get; init; }

  public int Limit { get; init; }

  public int Skip { get; init; }
}

public record Template
{
  public required TemplatePrincipal Principal { get; init; }
  public required UserPrincipal User { get; init; }
  public required IEnumerable<TemplateVersionPrincipal> Versions { get; init; }
  // Telemetry, non-user controlled
  public required TemplateInfo Info { get; init; }

}

public record TemplatePrincipal
{
  public required Guid Id { get; init; }

  public required string UserId { get; init; }
  // User Controlled, updatable, metadata
  public required TemplateMetadata Metadata { get; init; }

  // User Controlled, not-updatable, on create
  public required TemplateRecord Record { get; init; }
}

public record TemplateRecord
{
  public required string Name { get; init; }
}

public record TemplateInfo
{
  public required uint Downloads { get; init; }
  public required uint Stars { get; init; }
}

public record TemplateMetadata
{
  public required string Project { get; init; }
  public required string Source { get; init; }
  public required string Email { get; init; }
  public required string[] Tags { get; init; }
  public required string Description { get; init; }
  public required string Readme { get; init; }
}
