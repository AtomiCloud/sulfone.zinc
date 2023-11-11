using App.Modules.Cyan.API.V1.Models;
using App.Modules.Users.API.V1;
using Domain.Model;

namespace App.Modules.Cyan.API.V1.Mappers;

public static class ProcessorMapper
{
  public static (ProcessorRecord, ProcessorMetadata, ProcessorVersionRecord, ProcessorVersionProperty) ToDomain(
    this PushProcessorReq req) =>
  (
    new ProcessorRecord { Name = req.Name },
    new ProcessorMetadata
    {
      Project = req.Project,
      Source = req.Source,
      Email = req.Email,
      Tags = req.Tags,
      Description = req.Description,
      Readme = req.Readme
    },
    new ProcessorVersionRecord { Description = req.Description, },
    new ProcessorVersionProperty { DockerReference = req.DockerReference, DockerTag = req.DockerTag, }
  );

  public static (ProcessorRecord, ProcessorMetadata) ToDomain(this CreateProcessorReq req) =>
    (new ProcessorRecord { Name = req.Name },
      new ProcessorMetadata
      {
        Project = req.Project,
        Source = req.Source,
        Email = req.Email,
        Tags = req.Tags,
        Description = req.Description,
        Readme = req.Readme
      });

  public static ProcessorMetadata ToDomain(this UpdateProcessorReq req) =>
    new()
    {
      Project = req.Project,
      Source = req.Source,
      Email = req.Email,
      Tags = req.Tags,
      Description = req.Description,
      Readme = req.Readme
    };

  public static ProcessorSearch ToDomain(this SearchProcessorQuery query) =>
    new()
    {
      Owner = query.Owner,
      Search = query.Search,
      Limit = query.Limit ?? 20,
      Skip = query.Skip ?? 0,
    };

  public static ProcessorInfoResp ToResp(this ProcessorInfo info) => new(info.Downloads, info.Dependencies, info.Stars);

  public static ProcessorPrincipalResp ToResp(this ProcessorPrincipal principal) =>
    new(principal.Id, principal.Record.Name, principal.Metadata.Project,
      principal.Metadata.Source, principal.Metadata.Email, principal.Metadata.Tags,
      principal.Metadata.Description, principal.Metadata.Readme, principal.UserId);

  public static ProcessorResp ToResp(this Processor processor) =>
    new(processor.Principal.ToResp(), processor.Info.ToResp(), processor.User.ToResp(),
      processor.Versions.Select(v => v.ToResp()));
}

public static class ProcessorVersionMapper
{
  public static (ProcessorVersionProperty, ProcessorVersionRecord) ToDomain(this CreateProcessorVersionReq req) =>
    (new ProcessorVersionProperty { DockerReference = req.DockerReference, DockerTag = req.DockerTag },
      new ProcessorVersionRecord { Description = req.Description });

  public static ProcessorVersionRecord ToDomain(this UpdateProcessorVersionReq req) =>
    new() { Description = req.Description };

  public static ProcessorVersionSearch ToDomain(this SearchProcessorVersionQuery query) =>
    new() { Search = query.Search, Limit = query.Limit ?? 20, Skip = query.Skip ?? 0, };

  public static ProcessorVersionPrincipalResp ToResp(this ProcessorVersionPrincipal principal) =>
    new(principal.Id, principal.Version, principal.CreatedAt,
      principal.Record.Description, principal.Property.DockerReference,
      principal.Property.DockerTag);

  public static ProcessorVersionResp ToResp(this ProcessorVersion version) =>
    new(version.Principal.ToResp(), version.ProcessorPrincipal.ToResp());
}
