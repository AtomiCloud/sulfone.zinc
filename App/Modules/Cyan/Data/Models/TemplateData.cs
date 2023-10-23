using App.Modules.Users.Data;
using NpgsqlTypes;

namespace App.Modules.Cyan.Data.Models;

public record TemplateData
{
  public Guid Id { get; set; }

  public uint Downloads { get; set; }

  public string Name { get; set; } = string.Empty;

  public string Project { get; set; } = string.Empty;

  public string Source { get; set; } = string.Empty;

  public string Email { get; set; } = string.Empty;

  public string[] Tags { get; set; } = Array.Empty<string>();

  public string Description { get; set; } = string.Empty;

  public string Readme { get; set; } = string.Empty;

  public NpgsqlTsVector SearchVector { get; set; } = null!;

  // Foreign Keys
  public string UserId { get; set; } = string.Empty;

  public UserData User { get; set; } = null!;

  public IEnumerable<TemplateVersionData> Versions { get; set; } = null!;
  public IEnumerable<TemplateLikeData> Likes { get; set; } = null!;
}
