namespace Domain.Model;

public record ProcessorVersionSearch
{
  public string? Search { get; init; }

  public int Limit { get; init; }

  public int Skip { get; init; }
}

public record ProcessorVersion
{
  public required ProcessorVersionPrincipal Principal { get; init; }

  public required ProcessorPrincipal ProcessorPrincipal { get; init; }
}

public record ProcessorVersionRef(string Username, string Name, ulong? Version);

public record ProcessorVersionPrincipal
{
  public required Guid Id { get; init; }

  public required ulong Version { get; init; }

  public required DateTime CreatedAt { get; init; }

  public required ProcessorVersionRecord Record { get; init; }

  public required ProcessorVersionProperty Property { get; init; }
}

public record ProcessorVersionRecord
{
  public required string Description { get; init; }
}

public record ProcessorVersionProperty
{
  public required string DockerReference { get; init; }

  public required string DockerSha { get; init; }
}
