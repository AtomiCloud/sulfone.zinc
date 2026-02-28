namespace App.Modules.Cyan.Data.Models;

public record ResolverVersionData
{
  public Guid Id { get; set; }

  public ulong Version { get; set; }

  public DateTime CreatedAt { get; set; }

  public string Description { get; set; } = string.Empty;

  public string DockerReference { get; set; } = string.Empty;

  public string DockerTag { get; set; } = string.Empty;

  // Foreign Keys
  public Guid ResolverId { get; set; }

  public ResolverData Resolver { get; set; } = null!;

  public IEnumerable<TemplateResolverVersionData> Templates { get; set; } = null!;
}
