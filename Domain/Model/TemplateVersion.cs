namespace Domain.Model;

public record TemplateVersionSearch
{
  public string? Search { get; init; }

  public int Limit { get; init; }

  public int Skip { get; init; }
}

public record TemplateVersion
{
  public required TemplateVersionPrincipal Principal { get; init; }

  public required TemplatePrincipal TemplatePrincipal { get; init; }

  public required IEnumerable<PluginVersionPrincipal> Plugins { get; init; }

  public required IEnumerable<ProcessorVersionPrincipal> Processors { get; init; }
}

public record TemplateVersionPrincipal
{
  public required Guid Id { get; init; }

  public required ulong Version { get; init; }

  public required DateTime CreatedAt { get; init; }

  public required TemplateVersionRecord Record { get; init; }

  public required TemplateVersionProperty Property { get; init; }
}

public record TemplateVersionRecord
{
  public required string Description { get; init; }
}

public record TemplateVersionProperty
{
  public required string BlobDockerReference { get; init; }

  public required string BlobDockerTag { get; init; }

  public required string TemplateDockerReference { get; init; }

  public required string TemplateDockerTag { get; init; }
}
