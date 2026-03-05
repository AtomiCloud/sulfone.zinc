using System.Text.Json;
using Domain.Model;

namespace Domain.Model;

/// <summary>
/// Represents a template's reference to a resolver version with associated config and file patterns.
/// This is the domain model used when reading template-resolver relationships.
/// </summary>
/// <param name="Resolver">The resolved resolver version principal</param>
/// <param name="Config">Dynamic JSON configuration object for the resolver</param>
/// <param name="Files">Glob patterns for file matching (e.g., ["package.json", "**/tsconfig.json"])</param>
public record TemplateVersionResolverRef(
  ResolverVersionPrincipal Resolver,
  JsonElement Config,
  string[] Files
);
