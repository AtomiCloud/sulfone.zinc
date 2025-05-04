namespace App.Modules.Cyan.Data.Models;

public record TemplateProcessorVersionData
{
  public Guid Id { get; set; }

  public Guid TemplateId { get; set; }
  public TemplateVersionData Template { get; set; } = null!;

  public Guid ProcessorId { get; set; }
  public ProcessorVersionData Processor { get; set; } = null!;
}
