namespace App.Modules.Users.Data;

public record TokenData
{
  public Guid Id { get; set; }

  public string Name { get; set; } = string.Empty;

  public string ApiToken { get; set; } = string.Empty;

  public bool Revoked { get; set; } = false;

  public string UserId { get; set; } = string.Empty;
  public UserData User { get; set; } = new();
}
