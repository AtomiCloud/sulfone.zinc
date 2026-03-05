using System.Text.Json;
using Domain.Model;

namespace Domain.Service;

/// <summary>
/// Service-layer input type for passing resolver data from API to service.
/// Contains the unresolved reference and associated configuration.
/// </summary>
/// <param name="Resolver">Unresolved resolver reference (Username, Name, Version?) used for lookup</param>
/// <param name="Config">Dynamic JSON configuration object for the resolver</param>
/// <param name="Files">Glob patterns for file matching (e.g., ["package.json", "**/tsconfig.json"])</param>
public record TemplateVersionResolverInput(
  ResolverVersionRef Resolver,
  JsonElement Config,
  string[] Files
);
