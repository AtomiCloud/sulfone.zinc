namespace Domain;

public record User
{
  public required Guid Id { get; init; }
  public required string Name { get; init; }
  public required ushort Age { get; init; }
  public required string Email { get; init; }
}
