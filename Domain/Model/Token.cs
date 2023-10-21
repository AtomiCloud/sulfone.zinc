namespace Domain.Model;

public record Token
{
  public required TokenPrincipal Principal { get; init; }

  public required UserPrincipal UserPrincipal { get; init; }
}

public record TokenPrincipal
{
  public required Guid Id { get; init; }

  public required TokenRecord Record { get; init; }

  public required TokenProp Property { get; init; }
}

public record TokenRecord
{
  public required string Name { get; init; } = string.Empty;

  public required bool Revoked { get; init; } = false;
}

public record TokenProp
{
  public required string ApiToken { get; init; } = string.Empty;
}
