using System.Text.Json;

namespace App.Modules.Cyan.API.V1.Models;

// Request

public record SearchTemplateVersionQuery(string? Search, int? Limit, int? Skip);

public record CreateTemplateVersionReq(
  string Description,
  string[] Commands,
  TemplatePropertyReq? Properties,
  PluginReferenceReq[] Plugins,
  ProcessorReferenceReq[] Processors,
  TemplateReferenceReq[] Templates,
  ResolverReferenceReq[] Resolvers
);

public record PushTemplateReq(
  string Name,
  string Project,
  string Source,
  string Email,
  string[] Tags,
  string Description,
  string Readme,
  string VersionDescription,
  string[] Commands,
  TemplatePropertyReq? Properties,
  PluginReferenceReq[] Plugins,
  ProcessorReferenceReq[] Processors,
  TemplateReferenceReq[] Templates,
  ResolverReferenceReq[] Resolvers
);

public record TemplatePropertyReq(
  string BlobDockerReference,
  string BlobDockerTag,
  string TemplateDockerReference,
  string TemplateDockerTag
);

public record PluginReferenceReq(string Username, string Name, uint Version);

public record ProcessorReferenceReq(string Username, string Name, uint Version);

public record TemplateReferenceReq(
  string Username,
  string Name,
  uint Version,
  JsonElement PresetAnswers
);

public record ResolverReferenceReq(
  string Username,
  string Name,
  uint Version,
  JsonElement Config,
  string[] Files
);

public record UpdateTemplateVersionReq(string Description);

// Response

public record TemplatePropertyResp(
  string BlobDockerReference,
  string BlobDockerTag,
  string TemplateDockerReference,
  string TemplateDockerTag
);

public record TemplateVersionPrincipalResp(
  Guid Id,
  ulong Version,
  DateTime CreatedAt,
  string Description,
  TemplatePropertyResp? Properties,
  string[] Commands
);

public record TemplateVersionResp(
  TemplateVersionPrincipalResp Principal,
  TemplatePrincipalResp Template,
  IEnumerable<PluginVersionPrincipalResp> Plugins,
  IEnumerable<ProcessorVersionPrincipalResp> Processors,
  IEnumerable<TemplateVersionTemplateRefResp> Templates,
  IEnumerable<TemplateVersionResolverResp> Resolvers,
  string[] Commands
);

public record TemplateVersionResolverResp(
  Guid Id,
  ulong Version,
  DateTime CreatedAt,
  string Description,
  string DockerReference,
  string DockerTag,
  JsonElement Config,
  string[] Files
);

public record TemplateVersionTemplateRefResp(
  Guid Id,
  ulong Version,
  DateTime CreatedAt,
  string Description,
  TemplatePropertyResp? Properties,
  JsonElement PresetAnswers
);
