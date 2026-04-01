using CSharp_Result;
using Domain.Model;
using Domain.Repository;
using Microsoft.Extensions.Logging;

namespace Domain.Service;

public class ResolverService(
  IResolverRepository repo,
  ILogger<ResolverService> logger,
  IUserRepository user
) : IResolverService
{
  public Task<Result<IEnumerable<ResolverPrincipal>>> Search(ResolverSearch search)
  {
    return repo.Search(search);
  }

  public Task<Result<Resolver?>> Get(string userId, Guid id)
  {
    return repo.Get(userId, id);
  }

  public Task<Result<Resolver?>> Get(string owner, string name)
  {
    return repo.Get(owner, name);
  }

  public Task<Result<ResolverPrincipal>> Create(
    string userId,
    ResolverRecord record,
    ResolverMetadata metadata
  )
  {
    return repo.Create(userId, record, metadata);
  }

  public Task<Result<ResolverPrincipal?>> Update(string userId, Guid id, ResolverMetadata metadata)
  {
    return repo.Update(userId, id, metadata);
  }

  public Task<Result<ResolverPrincipal?>> Update(
    string userId,
    string name,
    ResolverMetadata metadata
  )
  {
    return repo.Update(userId, name, metadata);
  }

  public Task<Result<Unit?>> Delete(string userId, Guid id)
  {
    return repo.Delete(userId, id);
  }

  public Task<Result<Unit?>> Like(string likerId, string username, string name, bool like)
  {
    return repo.Like(likerId, username, name, like);
  }

  public Task<Result<IEnumerable<ResolverVersionPrincipal>>> SearchVersion(
    string username,
    string name,
    ResolverVersionSearch version
  )
  {
    return repo.SearchVersion(username, name, version);
  }

  public Task<Result<IEnumerable<ResolverVersionPrincipal>>> SearchVersion(
    string userId,
    Guid id,
    ResolverVersionSearch version
  )
  {
    return repo.SearchVersion(userId, id, version);
  }

  public async Task<Result<ResolverVersion?>> GetVersion(
    string username,
    string name,
    ulong version,
    bool bumpDownload
  )
  {
    if (bumpDownload)
    {
      return await repo.GetVersion(username, name, version)
        .DoAwait(
          DoType.Ignore,
          async _ =>
          {
            var r = await repo.IncrementDownload(username, name);
            if (r.IsFailure())
            {
              var err = r.FailureOrDefault();
              logger.LogError(
                err,
                "Failed to increment download when obtaining version for Resolver '{Username}/{Name}:{Version}'",
                username,
                name,
                version
              );
            }

            return r;
          }
        );
    }
    return await repo.GetVersion(username, name, version);
  }

  public async Task<Result<ResolverVersion?>> GetVersion(
    string username,
    string name,
    bool bumpDownload
  )
  {
    if (bumpDownload)
    {
      return await repo.GetVersion(username, name)
        .DoAwait(
          DoType.Ignore,
          async _ =>
          {
            var r = await repo.IncrementDownload(username, name);
            if (r.IsFailure())
            {
              var err = r.FailureOrDefault();
              logger.LogError(
                err,
                "Failed to increment download when obtaining version for Resolver '{Username}/{Name}'",
                username,
                name
              );
            }

            return r;
          }
        );
    }
    return await repo.GetVersion(username, name);
  }

  public Task<Result<ResolverVersion?>> GetVersion(string userId, Guid id, ulong version)
  {
    return repo.GetVersion(userId, id, version);
  }

  public Task<Result<ResolverVersion?>> GetVersionById(Guid versionId)
  {
    return repo.GetVersionById(versionId);
  }

  public Task<Result<ResolverVersionPrincipal?>> CreateVersion(
    string username,
    string name,
    ResolverVersionRecord record,
    ResolverVersionProperty property
  )
  {
    return repo.CreateVersion(username, name, record, property);
  }

  public Task<Result<ResolverVersionPrincipal?>> CreateVersion(
    string userId,
    Guid id,
    ResolverVersionRecord record,
    ResolverVersionProperty property
  )
  {
    return repo.CreateVersion(userId, id, record, property);
  }

  public Task<Result<ResolverVersionPrincipal?>> UpdateVersion(
    string userId,
    Guid id,
    ulong version,
    ResolverVersionRecord record
  )
  {
    return repo.UpdateVersion(userId, id, version, record);
  }

  public Task<Result<ResolverVersionPrincipal?>> UpdateVersion(
    string username,
    string name,
    ulong version,
    ResolverVersionRecord record
  )
  {
    return repo.UpdateVersion(username, name, version, record);
  }

  public async Task<Result<ResolverVersionPrincipal?>> Push(
    string username,
    ResolverRecord pRecord,
    ResolverMetadata metadata,
    ResolverVersionRecord record,
    ResolverVersionProperty property
  )
  {
    return await repo.Get(username, pRecord.Name)
      .ThenAwait(async p =>
      {
        if (p != null)
          return await repo.UpdateAndCreateVersion(
            username,
            pRecord.Name,
            metadata,
            record,
            property
          );
        return await user.GetByUsername(username)
          .ThenAwait(u => repo.Create(u!.Principal.Id, pRecord, metadata))
          .ThenAwait(_ => repo.CreateVersion(username, pRecord.Name, record, property));
      });
  }
}
