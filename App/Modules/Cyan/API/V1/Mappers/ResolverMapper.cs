using App.Modules.Cyan.API.V1.Models;
using App.Modules.Users.API.V1;
using Domain.Model;

namespace App.Modules.Cyan.API.V1.Mappers;

public static class ResolverMapper
{
  public static (
    ResolverRecord,
    ResolverMetadata,
    ResolverVersionRecord,
    ResolverVersionProperty
  ) ToDomain(this PushResolverReq req) =>
    (
      new ResolverRecord { Name = req.Name },
      new ResolverMetadata
      {
        Project = req.Project,
        Source = req.Source,
        Email = req.Email,
        Tags = req.Tags,
        Description = req.Description,
        Readme = req.Readme,
      },
      new ResolverVersionRecord { Description = req.VersionDescription },
      new ResolverVersionProperty
      {
        DockerReference = req.DockerReference,
        DockerTag = req.DockerTag,
      }
    );

  public static (ResolverRecord, ResolverMetadata) ToDomain(this CreateResolverReq req) =>
    (
      new ResolverRecord { Name = req.Name },
      new ResolverMetadata
      {
        Project = req.Project,
        Source = req.Source,
        Email = req.Email,
        Tags = req.Tags,
        Description = req.Description,
        Readme = req.Readme,
      }
    );

  public static ResolverMetadata ToDomain(this UpdateResolverReq req) =>
    new()
    {
      Project = req.Project,
      Source = req.Source,
      Email = req.Email,
      Tags = req.Tags,
      Description = req.Description,
      Readme = req.Readme,
    };

  public static ResolverSearch ToDomain(this SearchResolverQuery query) =>
    new()
    {
      Owner = query.Owner,
      Search = query.Search,
      Limit = query.Limit ?? 20,
      Skip = query.Skip ?? 0,
    };

  public static ResolverInfoResp ToResp(this ResolverInfo info) =>
    new(info.Downloads, info.Dependencies, info.Stars);

  public static ResolverPrincipalResp ToResp(this ResolverPrincipal principal) =>
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

  public static ResolverResp ToResp(this Resolver resolver) =>
    new(
      resolver.Principal.ToResp(),
      resolver.Info.ToResp(),
      resolver.User.ToResp(),
      resolver.Versions.Select(v => v.ToResp())
    );
}

public static class ResolverVersionMapper
{
  public static (ResolverVersionProperty, ResolverVersionRecord) ToDomain(
    this CreateResolverVersionReq req
  ) =>
    (
      new ResolverVersionProperty
      {
        DockerReference = req.DockerReference,
        DockerTag = req.DockerTag,
      },
      new ResolverVersionRecord { Description = req.Description }
    );

  public static ResolverVersionRecord ToDomain(this UpdateResolverVersionReq req) =>
    new() { Description = req.Description };

  public static ResolverVersionSearch ToDomain(this SearchResolverVersionQuery query) =>
    new()
    {
      Search = query.Search,
      Limit = query.Limit ?? 20,
      Skip = query.Skip ?? 0,
    };

  public static ResolverVersionPrincipalResp ToResp(this ResolverVersionPrincipal principal) =>
    new(
      principal.Id,
      principal.Version,
      principal.CreatedAt,
      principal.Record.Description,
      principal.Property.DockerReference,
      principal.Property.DockerTag
    );

  public static ResolverVersionResp ToResp(this ResolverVersion version) =>
    new(version.Principal.ToResp(), version.ResolverPrincipal.ToResp());
}
