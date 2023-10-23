namespace Domain.Model;

public record PluginSearch
{
  public string? Search { get; init; }

  public string? Owner { get; init; }

  public int Limit { get; init; }

  public int Skip { get; init; }
}


public record Plugin
{
  public required PluginPrincipal Principal { get; init; }

  public required UserPrincipal User { get; init; }

  public required IEnumerable<PluginVersionPrincipal> Versions { get; init; }

  // Telemetry, non-user controlled
  public required PluginInfo Info { get; init; }
}

public record PluginPrincipal
{
  public required Guid Id { get; init; }

  // User Controlled, updatable, metadata
  public required PluginMetadata Metadata { get; init; }

  // User Controlled, not-updatable, on create
  public required PluginRecord Record { get; init; }
}

public record PluginRecord
{
  public required string Name { get; init; }
}

public record PluginInfo
{
  public required uint Downloads { get; init; }

  public required uint Dependencies { get; init; }

  public required uint Stars { get; init; }
}

public record PluginMetadata
{
  public required string Project { get; init; }
  public required string Source { get; init; }
  public required string Email { get; init; }
  public required string[] Tags { get; init; }
  public required string Description { get; init; }
  public required string Readme { get; init; }
}
