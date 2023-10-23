using App.Modules.Cyan.Data.Models;
using App.Modules.Users.Data;
using Domain.Model;

namespace App.Modules.Cyan.Data.Mappers;

public static class PluginMapper
{
  public static PluginData HydrateData(this PluginData data, PluginMetadata metadata) =>
    data with
    {
      Project = metadata.Project,
      Source = metadata.Source,
      Email = metadata.Email,
      Tags = metadata.Tags,
      Description = metadata.Description,
      Readme = metadata.Readme,
    };

  public static PluginData HydrateData(this PluginData data, PluginRecord record) =>
    data with { Name = record.Name };

  public static PluginMetadata ToMetadata(this PluginData data) =>
    new()
    {
      Project = data.Project,
      Source = data.Source,
      Email = data.Email,
      Tags = data.Tags,
      Description = data.Description,
      Readme = data.Readme,
    };

  public static PluginRecord ToRecord(this PluginData data) =>
    new() { Name = data.Name };

  public static PluginPrincipal ToPrincipal(this PluginData data) =>
    new() { Id = data.Id, Metadata = data.ToMetadata(), Record = data.ToRecord(), };

  public static Plugin ToDomain(this PluginData data, PluginInfo info) =>
    new()
    {
      Principal = data.ToPrincipal(),
      User = data.User.ToPrincipal(),
      Versions = data.Versions.Select(x => x.ToPrincipal()).ToList(),
      Info = info
    };
}

public static class PluginVersionMapper
{
  public static PluginVersionData HydrateData(this PluginVersionData data, PluginVersionRecord record) =>
    data with { Description = record.Description, };

  public static PluginVersionData HydrateData(this PluginVersionData data, PluginVersionProperty record) =>
    data with { DockerReference = record.DockerReference, DockerSha = record.DockerSha, };

  public static PluginVersionProperty ToProperty(this PluginVersionData data) =>
    new() { DockerReference = data.DockerReference, DockerSha = data.DockerSha, };

  public static PluginVersionRecord ToRecord(this PluginVersionData data) =>
    new() { Description = data.Description, };

  public static PluginVersionPrincipal ToPrincipal(this PluginVersionData data) =>
    new()
    {
      Id = data.Id,
      Version = data.Version,
      CreatedAt = data.CreatedAt,
      Record = data.ToRecord(),
      Property = data.ToProperty(),
    };

  public static PluginVersion ToDomain(this PluginVersionData data) =>
    new() { Principal = data.ToPrincipal(), PluginPrincipal = data.Plugin.ToPrincipal(), };
}
