using App.Modules.Cyan.Data.Models;
using App.Modules.Users.Data;
using Domain.Model;

namespace App.Modules.Cyan.Data.Mappers;

public static class ProcessorMapper
{
  public static ProcessorData HydrateData(this ProcessorData data, ProcessorMetadata metadata) =>
    data with
    {
      Project = metadata.Project,
      Source = metadata.Source,
      Email = metadata.Email,
      Tags = metadata.Tags,
      Description = metadata.Description,
      Readme = metadata.Readme,
    };

  public static ProcessorData HydrateData(this ProcessorData data, ProcessorRecord record) =>
    data with { Name = record.Name };

  public static ProcessorMetadata ToMetadata(this ProcessorData data) =>
    new()
    {
      Project = data.Project,
      Source = data.Source,
      Email = data.Email,
      Tags = data.Tags,
      Description = data.Description,
      Readme = data.Readme,
    };

  public static ProcessorRecord ToRecord(this ProcessorData data) =>
    new() { Name = data.Name };

  public static ProcessorPrincipal ToPrincipal(this ProcessorData data) =>
    new()
    {
      Id = data.Id,
      Metadata = data.ToMetadata(),
      Record = data.ToRecord(),
      UserId = data.UserId,
    };

  public static Processor ToDomain(this ProcessorData data, ProcessorInfo info) =>
    new()
    {
      Principal = data.ToPrincipal(),
      User = data.User.ToPrincipal(),
      Versions = data.Versions.Select(x => x.ToPrincipal()).ToList(),
      Info = info,
    };
}

public static class ProcessorVersionMapper
{
  public static ProcessorVersionData HydrateData(this ProcessorVersionData data, ProcessorVersionRecord record) =>
    data with { Description = record.Description, };

  public static ProcessorVersionData HydrateData(this ProcessorVersionData data, ProcessorVersionProperty record) =>
    data with { DockerReference = record.DockerReference, DockerTag = record.DockerTag, };

  public static ProcessorVersionProperty ToProperty(this ProcessorVersionData data) =>
    new() { DockerReference = data.DockerReference, DockerTag = data.DockerTag, };

  public static ProcessorVersionRecord ToRecord(this ProcessorVersionData data) =>
    new() { Description = data.Description, };

  public static ProcessorVersionPrincipal ToPrincipal(this ProcessorVersionData data) =>
    new()
    {
      Id = data.Id,
      Version = data.Version,
      CreatedAt = data.CreatedAt,
      Record = data.ToRecord(),
      Property = data.ToProperty(),
    };

  public static ProcessorVersion ToDomain(this ProcessorVersionData data) =>
    new() { Principal = data.ToPrincipal(), ProcessorPrincipal = data.Processor.ToPrincipal(), };
}
