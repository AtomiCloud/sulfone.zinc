namespace App.Modules.Cyan.API.V1.Models;

public record SearchTemplateVersionQuery(string? Search, int? Limit, int? Skip);

public record CreateTemplateVersionReq(
  string Description,
  string BlobDockerReference,
  string BlobDockerTag,
  string TemplateDockerReference,
  string TemplateDockerTag,
  PluginReferenceReq[] Plugins,
  ProcessorReferenceReq[] Processors
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
  string BlobDockerReference,
  string BlobDockerTag,
  string TemplateDockerReference,
  string TemplateDockerTag,
  PluginReferenceReq[] Plugins,
  ProcessorReferenceReq[] Processors
);

public record PluginReferenceReq(string Username, string Name, uint Version);

public record ProcessorReferenceReq(string Username, string Name, uint Version);

public record UpdateTemplateVersionReq(string Description);

public record TemplateVersionPrincipalResp(
  Guid Id, ulong Version, DateTime CreatedAt,
  string Description,
  string BlobDockerReference, string BlobDockerTag,
  string TemplateDockerReference, string TemplateDockerTag
);

public record TemplateVersionResp(
  TemplateVersionPrincipalResp Principal,
  TemplatePrincipalResp Template,
  IEnumerable<PluginVersionPrincipalResp> Plugins,
  IEnumerable<ProcessorVersionPrincipalResp> Processors);
