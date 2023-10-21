namespace App.Modules.Users.Data;

public record UserData
{
  public string Id { get; set; } = string.Empty;

  public string Username { get; set; } = string.Empty;

  public IEnumerable<TokenData> Tokens { get; set; } = new List<TokenData>();
};
