namespace App.Modules.Cyan.Data.Models;

public record ProcessorVersionData
{
  public Guid Id { get; set; }

  public ulong Version { get; set; }

  public DateTime CreatedAt { get; set; }

  public string Description { get; set; } = string.Empty;

  public string DockerReference { get; set; } = string.Empty;

  public string DockerSha { get; set; } = string.Empty;

  // Foreign Keys
  public Guid ProcessorId { get; set; }

  public ProcessorData Processor { get; set; } = null!;

  public IEnumerable<TemplateProcessorVersionData> Templates { get; set; } = null!;
}
