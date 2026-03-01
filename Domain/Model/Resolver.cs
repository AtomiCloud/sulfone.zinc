namespace Domain.Model;

public record ResolverSearch
{
  public string? Owner { get; init; }
  public string? Search { get; init; }

  public int Limit { get; init; }

  public int Skip { get; init; }
}

public record Resolver
{
  public required ResolverPrincipal Principal { get; init; }
  public required UserPrincipal User { get; init; }
  public required IEnumerable<ResolverVersionPrincipal> Versions { get; init; }

  // Telemetry, non-user controlled
  public required ResolverInfo Info { get; init; }
}

public record ResolverPrincipal
{
  public required Guid Id { get; init; }

  public required string UserId { get; init; }

  // User Controlled, updatable, metadata
  public required ResolverMetadata Metadata { get; init; }

  // User Controlled, not-updatable, on create
  public required ResolverRecord Record { get; init; }
}

public record ResolverInfo
{
  public required uint Downloads { get; init; }

  public required uint Dependencies { get; init; }

  public required uint Stars { get; init; }
}

public record ResolverRecord
{
  public required string Name { get; init; }
}

public record ResolverMetadata
{
  public required string Project { get; init; }
  public required string Source { get; init; }
  public required string Email { get; init; }
  public required string[] Tags { get; init; }
  public required string Description { get; init; }
  public required string Readme { get; init; }
}
