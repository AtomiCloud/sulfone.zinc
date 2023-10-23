namespace App.Modules.Cyan.Data.Models;

public record TemplatePluginVersionData
{
  public Guid Id { get; set; }

  public Guid TemplateId { get; set; }
  public TemplateVersionData Template { get; set; } = null!;

  public Guid PluginId { get; set; }
  public PluginVersionData Plugin { get; set; } = null!;
}
