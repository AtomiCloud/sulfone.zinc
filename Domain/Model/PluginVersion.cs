namespace Domain.Model;

public record PluginVersionSearch
{
  public string? Search { get; init; }

  public int Limit { get; init; }

  public int Skip { get; init; }
}


public record PluginVersion
{
  public required PluginVersionPrincipal Principal { get; init; }

  public required PluginPrincipal PluginPrincipal { get; init; }
}

public record PluginVersionRef(string Username, string Name, ulong? Version);

public record PluginVersionPrincipal
{
  public required Guid Id { get; init; }

  public required ulong Version { get; init; }

  public required DateTime CreatedAt { get; init; }

  public required PluginVersionRecord Record { get; init; }

  public required PluginVersionProperty Property { get; init; }
}

public record PluginVersionRecord
{
  public required string Description { get; init; }
}

public record PluginVersionProperty
{

  public required string DockerReference { get; init; }

  public required string DockerSha { get; init; }

}
