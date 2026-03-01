namespace App.Modules.Cyan.Data.Models;

public record TemplateResolverVersionData
{
  public Guid Id { get; set; }

  public Guid TemplateId { get; set; }
  public TemplateVersionData Template { get; set; } = null!;

  public Guid ResolverId { get; set; }
  public ResolverVersionData Resolver { get; set; } = null!;
}
