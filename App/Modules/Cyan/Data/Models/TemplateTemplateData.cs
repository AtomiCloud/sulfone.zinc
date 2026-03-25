namespace App.Modules.Cyan.Data.Models;

public record TemplateTemplateVersionData
{
  public Guid Id { get; set; }

  public Guid TemplateId { get; set; }
  public TemplateVersionData Template { get; set; } = null!;

  public Guid TemplateRefId { get; set; }
  public TemplateVersionData TemplateRef { get; set; } = null!;

  /// <summary>
  /// PresetAnswers stores dynamic JSON preset answer configurations for the sub-template.
  /// Stored as JSON string in TEXT column.
  /// </summary>
  public string PresetAnswers { get; set; } = "{}";
}
