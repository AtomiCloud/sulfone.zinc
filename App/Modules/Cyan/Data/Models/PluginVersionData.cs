namespace App.Modules.Cyan.Data.Models;

public record PluginVersionData
{
  public Guid Id { get; set; }

  public ulong Version { get; set; }

  public DateTime CreatedAt { get; set; }

  public string Description { get; set; } = string.Empty;

  public string DockerReference { get; set; } = string.Empty;

  public string DockerTag { get; set; } = string.Empty;

  // Foreign Keys
  public Guid PluginId { get; set; }

  public PluginData Plugin { get; set; } = null!;

  public IEnumerable<TemplatePluginVersionData> Templates { get; set; } = null!;

};
