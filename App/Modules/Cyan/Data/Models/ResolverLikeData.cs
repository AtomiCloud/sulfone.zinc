using App.Modules.Users.Data;

namespace App.Modules.Cyan.Data.Models;

public record ResolverLikeData
{
  public Guid Id { get; set; }

  public Guid ResolverId { get; set; }
  public ResolverData Resolver { get; set; } = null!;

  public string UserId { get; set; } = string.Empty;
  public UserData User { get; set; } = null!;
}
