namespace App.Modules.Users.Data;

public record UserData
{
  public required Guid Id { get; init; }

  public required string Name { get; init; }

  public required string Email { get; init; }

  public required ushort Age { get; init; }
};
