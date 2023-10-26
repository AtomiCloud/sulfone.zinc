using App.Modules.Cyan.Data.Models;

namespace App.Modules.Users.Data;

public record UserData
{
  public string Id { get; set; } = string.Empty;

  public string Username { get; set; } = string.Empty;

  // Foreign Keys
  public IEnumerable<TokenData> Tokens { get; set; } = null!;

  public IEnumerable<TemplateData> Templates { get; set; } = null!;

  public IEnumerable<PluginData> Plugins { get; set; } = null!;

  public IEnumerable<ProcessorData> Processors { get; set; } = null!;

  public IEnumerable<TemplateLikeData> TemplateLikes { get; set; } = null!;

  public IEnumerable<PluginLikeData> PluginLikes { get; set; } = null!;

  public IEnumerable<ProcessorLikeData> ProcessorLikes { get; set; } = null!;
};
