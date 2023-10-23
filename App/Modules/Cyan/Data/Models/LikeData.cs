using App.Modules.Users.Data;

namespace App.Modules.Cyan.Data.Models;

public record TemplateLikeData
{
  public Guid Id { get; set; }

  public Guid TemplateId { get; set; }
  public TemplateData Template { get; set; } = null!;

  public string UserId { get; set; } = string.Empty;
  public UserData User { get; set; } = null!;
}

public record PluginLikeData
{
  public Guid Id { get; set; }

  public Guid PluginId { get; set; }
  public PluginData Plugin { get; set; } = null!;

  public string UserId { get; set; } = string.Empty;
  public UserData User { get; set; } = null!;
}

public record ProcessorLikeData
{
  public Guid Id { get; set; }

  public Guid ProcessorId { get; set; }
  public ProcessorData Processor { get; set; } = null!;

  public string UserId { get; set; } = string.Empty;
  public UserData User { get; set; } = null!;
}
