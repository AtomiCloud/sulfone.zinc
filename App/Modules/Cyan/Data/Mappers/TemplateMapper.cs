using App.Modules.Cyan.Data.Models;
using App.Modules.Users.Data;
using Domain.Model;

namespace App.Modules.Cyan.Data.Mappers;

public static class TemplateMapper
{
  public static TemplateData HydrateData(this TemplateData data, TemplateMetadata metadata) =>
    data with
    {
      Project = metadata.Project,
      Source = metadata.Source,
      Email = metadata.Email,
      Tags = metadata.Tags,
      Description = metadata.Description,
      Readme = metadata.Readme,
    };

  public static TemplateData HydrateData(this TemplateData data, TemplateRecord record) =>
    data with { Name = record.Name };

  public static TemplateMetadata ToMetadata(this TemplateData data) =>
    new()
    {
      Project = data.Project,
      Source = data.Source,
      Email = data.Email,
      Tags = data.Tags,
      Description = data.Description,
      Readme = data.Readme,
    };

  public static TemplateRecord ToRecord(this TemplateData data) =>
    new() { Name = data.Name };

  public static TemplatePrincipal ToPrincipal(this TemplateData data) =>
    new()
    {
      Id = data.Id,
      Metadata = data.ToMetadata(),
      Record = data.ToRecord(),
      UserId = data.UserId,
    };

  public static Template ToDomain(this TemplateData data, TemplateInfo info) =>
    new()
    {
      Principal = data.ToPrincipal(),
      User = data.User.ToPrincipal(),
      Versions = data.Versions.Select(x => x.ToPrincipal()).ToList(),
      Info = info,
    };
}

public static class TemplateVersionMapper
{
  public static TemplateVersionData HydrateData(this TemplateVersionData data, TemplateVersionRecord record) =>
    data with { Description = record.Description, };

  public static TemplateVersionData HydrateData(this TemplateVersionData data, TemplateVersionProperty record) =>
    data with
    {
      BlobDockerReference = record.BlobDockerReference,
      BlobDockerSha = record.BlobDockerSha,
      TemplateDockerReference = record.TemplateDockerReference,
      TemplateDockerSha = record.TemplateDockerSha,
    };

  public static TemplateVersionProperty ToProperty(this TemplateVersionData data) =>
    new()
    {
      BlobDockerReference = data.BlobDockerReference,
      BlobDockerSha = data.BlobDockerSha,
      TemplateDockerReference = data.TemplateDockerReference,
      TemplateDockerSha = data.TemplateDockerSha,
    };

  public static TemplateVersionRecord ToRecord(this TemplateVersionData data) =>
    new() { Description = data.Description, };

  public static TemplateVersionPrincipal ToPrincipal(this TemplateVersionData data) =>
    new()
    {
      Id = data.Id,
      Version = data.Version,
      CreatedAt = data.CreatedAt,
      Record = data.ToRecord(),
      Property = data.ToProperty(),
    };

  public static TemplateVersion ToDomain(this TemplateVersionData data) =>
    new()
    {
      Principal = data.ToPrincipal(),
      TemplatePrincipal = data.Template.ToPrincipal(),
      Plugins = data.Plugins.Select(x => x.Plugin.ToPrincipal()).ToList(),
      Processors = data.Processors.Select(x => x.Processor.ToPrincipal()).ToList(),
    };
}
