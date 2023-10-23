namespace Domain.Model;

public record ProcessorSearch
{
  public string? Owner { get; init; }
  public string? Search { get; init; }

  public int Limit { get; init; }

  public int Skip { get; init; }
}

public record Processor
{
  public required ProcessorPrincipal Principal { get; init; }
  public required UserPrincipal User { get; init; }
  public required IEnumerable<ProcessorVersionPrincipal> Versions { get; init; }

  // Telemetry, non-user controlled
  public required ProcessorInfo Info { get; init; }
}

public record ProcessorPrincipal
{
  public required Guid Id { get; init; }

  // User Controlled, updatable, metadata
  public required ProcessorMetadata Metadata { get; init; }

  // User Controlled, not-updatable, on create
  public required ProcessorRecord Record { get; init; }
}

public record ProcessorInfo
{
  public required uint Downloads { get; init; }

  public required uint Dependencies { get; init; }

  public required uint Stars { get; init; }
}

public record ProcessorRecord
{
  public required string Name { get; init; }
}

public record ProcessorMetadata
{
  public required string Project { get; init; }
  public required string Source { get; init; }
  public required string Email { get; init; }
  public required string[] Tags { get; init; }
  public required string Description { get; init; }
  public required string Readme { get; init; }
}
