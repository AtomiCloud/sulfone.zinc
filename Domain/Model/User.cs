namespace Domain;


public record UserSearch
{
  public string? Id { get; init; }
  public string? Username { get; init; }
  public string? Sub { get; init; }

  public int Limit { get; init; }

  public int Skip { get; init; }
}

public record UserPrincipal
{
  public required Guid Id { get; init; }
  public required string Sub { get; init; }
  public required UserRecord Record { get; init; }
}

public record UserRecord
{
  public required string Username { get; init; }
}
