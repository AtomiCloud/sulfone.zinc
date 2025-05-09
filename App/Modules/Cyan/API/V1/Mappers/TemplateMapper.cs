using App.Modules.Cyan.API.V1.Models;
using App.Modules.Users.API.V1;
using Domain.Model;

namespace App.Modules.Cyan.API.V1.Mappers;

public static class TemplateMapper
{
  public static (
    TemplateRecord,
    TemplateMetadata,
    TemplateVersionRecord,
    TemplateVersionProperty
  ) ToDomain(this PushTemplateReq req) =>
    (
      new TemplateRecord { Name = req.Name },
      new TemplateMetadata
      {
        Project = req.Project,
        Source = req.Source,
        Email = req.Email,
        Tags = req.Tags,
        Description = req.Description,
        Readme = req.Readme,
      },
      new TemplateVersionRecord { Description = req.Description },
      new TemplateVersionProperty
      {
        BlobDockerReference = req.BlobDockerReference,
        BlobDockerTag = req.BlobDockerTag,
        TemplateDockerReference = req.TemplateDockerReference,
        TemplateDockerTag = req.TemplateDockerTag,
      }
    );

  public static (TemplateRecord, TemplateMetadata) ToDomain(this CreateTemplateReq req) =>
    (
      new TemplateRecord { Name = req.Name },
      new TemplateMetadata
      {
        Project = req.Project,
        Source = req.Source,
        Email = req.Email,
        Tags = req.Tags,
        Description = req.Description,
        Readme = req.Readme,
      }
    );

  public static TemplateMetadata ToDomain(this UpdateTemplateReq req) =>
    new()
    {
      Project = req.Project,
      Source = req.Source,
      Email = req.Email,
      Tags = req.Tags,
      Description = req.Description,
      Readme = req.Readme,
    };

  public static TemplateSearch ToDomain(this SearchTemplateQuery query) =>
    new()
    {
      Owner = query.Owner,
      Search = query.Search,
      Limit = query.Limit ?? 20,
      Skip = query.Skip ?? 0,
    };

  public static TemplateInfoResp ToResp(this TemplateInfo info) => new(info.Downloads, info.Stars);

  public static TemplatePrincipalResp ToResp(this TemplatePrincipal principal) =>
    new(
      principal.Id,
      principal.Record.Name,
      principal.Metadata.Project,
      principal.Metadata.Source,
      principal.Metadata.Email,
      principal.Metadata.Tags,
      principal.Metadata.Description,
      principal.Metadata.Readme,
      principal.UserId
    );

  public static TemplateResp ToResp(this Template template) =>
    new(
      template.Principal.ToResp(),
      template.Info.ToResp(),
      template.User.ToResp(),
      template.Versions.Select(v => v.ToResp())
    );
}

public static class TemplateVersionMapper
{
  public static TemplateVersionRecord ToRecord(this CreateTemplateVersionReq req) =>
    new() { Description = req.Description };

  public static TemplateVersionProperty ToProperty(this CreateTemplateVersionReq req) =>
    new()
    {
      BlobDockerReference = req.BlobDockerReference,
      BlobDockerTag = req.BlobDockerTag,
      TemplateDockerReference = req.TemplateDockerReference,
      TemplateDockerTag = req.TemplateDockerTag,
    };

  public static TemplateVersionRecord ToDomain(this UpdateTemplateVersionReq req) =>
    new() { Description = req.Description };

  public static TemplateVersionSearch ToDomain(this SearchTemplateVersionQuery query) =>
    new()
    {
      Search = query.Search,
      Limit = query.Limit ?? 20,
      Skip = query.Skip ?? 0,
    };

  public static PluginVersionRef ToDomain(this PluginReferenceReq req) =>
    new(req.Username, req.Name, req.Version == 0 ? null : req.Version);

  public static ProcessorVersionRef ToDomain(this ProcessorReferenceReq req) =>
    new(req.Username, req.Name, req.Version == 0 ? null : req.Version);

  public static TemplateVersionRef ToDomain(this TemplateReferenceReq req) =>
    new(req.Username, req.Name, req.Version == 0 ? null : req.Version);

  public static TemplateVersionPrincipalResp ToResp(this TemplateVersionPrincipal principal) =>
    new(
      principal.Id,
      principal.Version,
      principal.CreatedAt,
      principal.Record.Description,
      principal.Property.BlobDockerReference,
      principal.Property.BlobDockerTag,
      principal.Property.TemplateDockerReference,
      principal.Property.TemplateDockerTag
    );

  public static TemplateVersionResp ToResp(this TemplateVersion version) =>
    new(
      version.Principal.ToResp(),
      version.TemplatePrincipal.ToResp(),
      version.Plugins.Select(x => x.ToResp()),
      version.Processors.Select(x => x.ToResp()),
      version.Templates.Select(x => x.ToResp())
    );
}
