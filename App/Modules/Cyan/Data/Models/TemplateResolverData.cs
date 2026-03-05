namespace App.Modules.Cyan.Data.Models;

public record TemplateResolverVersionData
{
  public Guid Id { get; set; }

  public Guid TemplateId { get; set; }
  public TemplateVersionData Template { get; set; } = null!;

  public Guid ResolverId { get; set; }
  public ResolverVersionData Resolver { get; set; } = null!;

  /// <summary>
  /// Config field stores dynamic JSON configuration for the resolver.
  /// Stored as JSON string in TEXT column ( no need for JSON indexing).
  /// </summary>
  public string Config { get; set; } = "{}";

  /// <summary>
  /// Files field stores glob patterns for file matching
  /// Stored as PostgreSQL TEXT[] array
  /// </summary>
  public string[] Files { get; set; } = Array.Empty<string>();
}
