using App.Modules.Cyan.Data.Models;
using App.Modules.Users.Data;
using Domain.Model;

namespace App.Modules.Cyan.Data.Mappers;

public static class ResolverMapper
{
  public static ResolverData HydrateData(this ResolverData data, ResolverMetadata metadata) =>
    data with
    {
      Project = metadata.Project,
      Source = metadata.Source,
      Email = metadata.Email,
      Tags = metadata.Tags,
      Description = metadata.Description,
      Readme = metadata.Readme,
    };

  public static ResolverData HydrateData(this ResolverData data, ResolverRecord record) =>
    data with
    {
      Name = record.Name,
    };

  public static ResolverMetadata ToMetadata(this ResolverData data) =>
    new()
    {
      Project = data.Project,
      Source = data.Source,
      Email = data.Email,
      Tags = data.Tags,
      Description = data.Description,
      Readme = data.Readme,
    };

  public static ResolverRecord ToRecord(this ResolverData data) => new() { Name = data.Name };

  public static ResolverPrincipal ToPrincipal(this ResolverData data) =>
    new()
    {
      Id = data.Id,
      Metadata = data.ToMetadata(),
      Record = data.ToRecord(),
      UserId = data.UserId,
    };

  public static Resolver ToDomain(this ResolverData data, ResolverInfo info) =>
    new()
    {
      Principal = data.ToPrincipal(),
      User = data.User.ToPrincipal(),
      Versions = data.Versions.Select(x => x.ToPrincipal()).ToList(),
      Info = info,
    };
}

public static class ResolverVersionMapper
{
  public static ResolverVersionData HydrateData(
    this ResolverVersionData data,
    ResolverVersionRecord record
  ) => data with { Description = record.Description };

  public static ResolverVersionData HydrateData(
    this ResolverVersionData data,
    ResolverVersionProperty record
  ) => data with { DockerReference = record.DockerReference, DockerTag = record.DockerTag };

  public static ResolverVersionProperty ToProperty(this ResolverVersionData data) =>
    new() { DockerReference = data.DockerReference, DockerTag = data.DockerTag };

  public static ResolverVersionRecord ToRecord(this ResolverVersionData data) =>
    new() { Description = data.Description };

  public static ResolverVersionPrincipal ToPrincipal(this ResolverVersionData data) =>
    new()
    {
      Id = data.Id,
      Version = data.Version,
      CreatedAt = data.CreatedAt,
      Record = data.ToRecord(),
      Property = data.ToProperty(),
    };

  public static ResolverVersion ToDomain(this ResolverVersionData data) =>
    new() { Principal = data.ToPrincipal(), ResolverPrincipal = data.Resolver.ToPrincipal() };
}
