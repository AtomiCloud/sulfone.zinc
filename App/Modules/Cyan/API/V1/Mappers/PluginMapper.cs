using App.Modules.Cyan.API.V1.Models;
using App.Modules.Users.API.V1;
using Domain.Model;

namespace App.Modules.Cyan.API.V1.Mappers;

public static class PluginMapper
{
  public static (PluginRecord, PluginMetadata, PluginVersionRecord, PluginVersionProperty) ToDomain(
    this PushPluginReq req
  ) =>
    (
      new PluginRecord { Name = req.Name },
      new PluginMetadata
      {
        Project = req.Project,
        Source = req.Source,
        Email = req.Email,
        Tags = req.Tags,
        Description = req.Description,
        Readme = req.Readme,
      },
      new PluginVersionRecord { Description = req.Description },
      new PluginVersionProperty { DockerReference = req.DockerReference, DockerTag = req.DockerTag }
    );

  public static (PluginRecord, PluginMetadata) ToDomain(this CreatePluginReq req) =>
    (
      new PluginRecord { Name = req.Name },
      new PluginMetadata
      {
        Project = req.Project,
        Source = req.Source,
        Email = req.Email,
        Tags = req.Tags,
        Description = req.Description,
        Readme = req.Readme,
      }
    );

  public static PluginMetadata ToDomain(this UpdatePluginReq req) =>
    new()
    {
      Project = req.Project,
      Source = req.Source,
      Email = req.Email,
      Tags = req.Tags,
      Description = req.Description,
      Readme = req.Readme,
    };

  public static PluginSearch ToDomain(this SearchPluginQuery query) =>
    new()
    {
      Owner = query.Owner,
      Search = query.Search,
      Limit = query.Limit ?? 20,
      Skip = query.Skip ?? 0,
    };

  public static PluginInfoResp ToResp(this PluginInfo info) =>
    new(info.Downloads, info.Dependencies, info.Stars);

  public static PluginPrincipalResp ToResp(this PluginPrincipal principal) =>
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

  public static PluginResp ToResp(this Plugin plugin) =>
    new(
      plugin.Principal.ToResp(),
      plugin.Info.ToResp(),
      plugin.User.ToResp(),
      plugin.Versions.Select(v => v.ToResp())
    );
}

public static class PluginVersionMapper
{
  public static (PluginVersionProperty, PluginVersionRecord) ToDomain(
    this CreatePluginVersionReq req
  ) =>
    (
      new PluginVersionProperty
      {
        DockerReference = req.DockerReference,
        DockerTag = req.DockerTag,
      },
      new PluginVersionRecord { Description = req.Description }
    );

  public static PluginVersionRecord ToDomain(this UpdatePluginVersionReq req) =>
    new() { Description = req.Description };

  public static PluginVersionSearch ToDomain(this SearchPluginVersionQuery query) =>
    new()
    {
      Search = query.Search,
      Limit = query.Limit ?? 20,
      Skip = query.Skip ?? 0,
    };

  public static PluginVersionPrincipalResp ToResp(this PluginVersionPrincipal principal) =>
    new(
      principal.Id,
      principal.Version,
      principal.CreatedAt,
      principal.Record.Description,
      principal.Property.DockerReference,
      principal.Property.DockerTag
    );

  public static PluginVersionResp ToResp(this PluginVersion version) =>
    new(version.Principal.ToResp(), version.PluginPrincipal.ToResp());
}
