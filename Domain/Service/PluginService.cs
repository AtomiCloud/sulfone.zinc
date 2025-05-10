using CSharp_Result;
using Domain.Model;
using Domain.Repository;
using Microsoft.Extensions.Logging;

namespace Domain.Service;

public class PluginService(
  IPluginRepository repo,
  ILogger<PluginService> logger,
  IUserRepository user
) : IPluginService
{
  public Task<Result<IEnumerable<PluginPrincipal>>> Search(PluginSearch search)
  {
    return repo.Search(search);
  }

  public Task<Result<Plugin?>> Get(string userId, Guid id)
  {
    return repo.Get(userId, id);
  }

  public Task<Result<Plugin?>> Get(string owner, string name)
  {
    return repo.Get(owner, name);
  }

  public Task<Result<PluginPrincipal>> Create(
    string userId,
    PluginRecord record,
    PluginMetadata metadata
  )
  {
    return repo.Create(userId, record, metadata);
  }

  public Task<Result<PluginPrincipal?>> Update(string userId, Guid id, PluginMetadata metadata)
  {
    return repo.Update(userId, id, metadata);
  }

  public Task<Result<PluginPrincipal?>> Update(string userId, string name, PluginMetadata metadata)
  {
    return repo.Update(userId, name, metadata);
  }

  public Task<Result<Unit?>> Like(string likerId, string userId, string name, bool like)
  {
    return repo.Like(likerId, userId, name, like);
  }

  public Task<Result<Unit?>> Delete(string userId, Guid id)
  {
    return repo.Delete(userId, id);
  }

  public Task<Result<IEnumerable<PluginVersionPrincipal>>> SearchVersion(
    string userId,
    string name,
    PluginVersionSearch version
  )
  {
    return repo.SearchVersion(userId, name, version);
  }

  public Task<Result<IEnumerable<PluginVersionPrincipal>>> SearchVersion(
    string userId,
    Guid id,
    PluginVersionSearch version
  )
  {
    return repo.SearchVersion(userId, id, version);
  }

  public async Task<Result<PluginVersion?>> GetVersion(
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
                "Failed to increment download when obtaining version for Plugin '{Username}/{Name}:{Version}'",
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

  public async Task<Result<PluginVersion?>> GetVersion(
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
                "Failed to increment download when obtaining version for Plugin '{Username}/{Name}'",
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

  public Task<Result<PluginVersion?>> GetVersion(string userId, Guid id, ulong version)
  {
    return repo.GetVersion(userId, id, version);
  }

  public Task<Result<PluginVersion?>> GetVersionById(Guid versionId)
  {
    return repo.GetVersionById(versionId);
  }

  public Task<Result<PluginVersionPrincipal?>> CreateVersion(
    string userId,
    string name,
    PluginVersionRecord record,
    PluginVersionProperty property
  )
  {
    return repo.CreateVersion(userId, name, record, property);
  }

  public async Task<Result<PluginVersionPrincipal?>> Push(
    string username,
    PluginRecord pRecord,
    PluginMetadata metadata,
    PluginVersionRecord record,
    PluginVersionProperty property
  )
  {
    return await repo.Get(username, pRecord.Name)
      .ThenAwait(async p =>
      {
        if (p != null)
          return p.Principal.ToResult();
        return await user.GetByUsername(username)
          .ThenAwait(u => repo.Create(u!.Principal.Id, pRecord, metadata));
      })
      .ThenAwait(x => repo.CreateVersion(username, pRecord.Name, record, property));
  }

  public Task<Result<PluginVersionPrincipal?>> CreateVersion(
    string userId,
    Guid id,
    PluginVersionRecord record,
    PluginVersionProperty property
  )
  {
    return repo.CreateVersion(userId, id, record, property);
  }

  public Task<Result<PluginVersionPrincipal?>> UpdateVersion(
    string userId,
    Guid id,
    ulong version,
    PluginVersionRecord record
  )
  {
    return repo.UpdateVersion(userId, id, version, record);
  }

  public Task<Result<PluginVersionPrincipal?>> UpdateVersion(
    string userId,
    string name,
    ulong version,
    PluginVersionRecord record
  )
  {
    return repo.UpdateVersion(userId, name, version, record);
  }
}
