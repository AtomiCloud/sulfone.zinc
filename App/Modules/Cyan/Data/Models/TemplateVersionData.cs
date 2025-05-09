namespace App.Modules.Cyan.Data.Models;

public record TemplateVersionData
{
  public Guid Id { get; set; }

  public ulong Version { get; set; }

  public DateTime CreatedAt { get; set; }

  public string Description { get; set; } = string.Empty;

  public string BlobDockerReference { get; set; } = string.Empty;

  public string BlobDockerTag { get; set; } = string.Empty;

  public string TemplateDockerReference { get; set; } = string.Empty;

  public string TemplateDockerTag { get; set; } = string.Empty;

  // Foreign Keys
  public Guid TemplateId { get; set; }

  public TemplateData Template { get; set; } = null!;

  public IEnumerable<TemplateProcessorVersionData> Processors { get; set; } = null!;

  public IEnumerable<TemplatePluginVersionData> Plugins { get; set; } = null!;

  public IEnumerable<TemplateTemplateVersionData> Templates { get; set; } = null!;

  public IEnumerable<TemplateTemplateVersionData> TemplateRefs { get; set; } = null!;
}
