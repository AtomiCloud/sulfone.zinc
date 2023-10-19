namespace App.Modules.Users.Data;

public record UserData
{
  public Guid Id { get; set; }

  public string Sub { get; set; } = string.Empty;

  public string Username { get; set; } = string.Empty;
};
