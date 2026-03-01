namespace Domain.Model;

public record ResolverVersionSearch
{
  public string? Search { get; init; }

  public int Limit { get; init; }

  public int Skip { get; init; }
}

public record ResolverVersion
{
  public required ResolverVersionPrincipal Principal { get; init; }

  public required ResolverPrincipal ResolverPrincipal { get; init; }
}

public record ResolverVersionRef(string Username, string Name, ulong? Version);

public record ResolverVersionPrincipal
{
  public required Guid Id { get; init; }

  public required ulong Version { get; init; }

  public required DateTime CreatedAt { get; init; }

  public required ResolverVersionRecord Record { get; init; }

  public required ResolverVersionProperty Property { get; init; }
}

public record ResolverVersionRecord
{
  public required string Description { get; init; }
}

public record ResolverVersionProperty
{
  public required string DockerReference { get; init; }

  public required string DockerTag { get; init; }
}
