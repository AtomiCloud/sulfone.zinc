using CSharp_Result;
using Domain.Model;
using Domain.Repository;
using Microsoft.Extensions.Logging;

namespace Domain.Service;

public class PluginService : IPluginService
{
  private readonly IPluginRepository _repo;
  private readonly IUserRepository _user;
  private readonly ILogger<PluginService> _logger;

  public PluginService(IPluginRepository repo, ILogger<PluginService> logger, IUserRepository user)
  {
    this._logger = logger;
    this._user = user;
    this._repo = repo;
  }

  public Task<Result<IEnumerable<PluginPrincipal>>> Search(PluginSearch search)
  {
    return this._repo.Search(search);
  }

  public Task<Result<Plugin?>> Get(string userId, Guid id)
  {
    return this._repo.Get(userId, id);
  }

  public Task<Result<Plugin?>> Get(string owner, string name)
  {
    return this._repo.Get(owner, name);
  }

  public Task<Result<PluginPrincipal>> Create(string userId, PluginRecord record, PluginMetadata metadata)
  {
    return this._repo.Create(userId, record, metadata);
  }

  public Task<Result<PluginPrincipal?>> Update(string userId, Guid id, PluginMetadata metadata)
  {
    return this._repo.Update(userId, id, metadata);
  }

  public Task<Result<PluginPrincipal?>> Update(string userId, string name, PluginMetadata metadata)
  {
    return this._repo.Update(userId, name, metadata);
  }

  public Task<Result<Unit?>> Like(string likerId, string userId, string name, bool like)
  {
    return this._repo.Like(likerId, userId, name, like);
  }

  public Task<Result<Unit?>> Delete(string userId, Guid id)
  {
    return this._repo.Delete(userId, id);
  }

  public Task<Result<IEnumerable<PluginVersionPrincipal>>> SearchVersion(string userId, string name,
    PluginVersionSearch version)
  {
    return this._repo.SearchVersion(userId, name, version);
  }

  public Task<Result<IEnumerable<PluginVersionPrincipal>>> SearchVersion(string userId, Guid id,
    PluginVersionSearch version)
  {
    return this._repo.SearchVersion(userId, id, version);
  }

  public async Task<Result<PluginVersion?>> GetVersion(string username, string name, ulong version,
    bool bumpDownload)
  {
    if (bumpDownload)
    {
      return await this._repo.GetVersion(username, name, version)
        .DoAwait(DoType.Ignore, async _ =>
        {
          var r = await this._repo.IncrementDownload(username, name);
          if (r.IsFailure())
          {
            var err = r.FailureOrDefault();
            this._logger.LogError(err,
              "Failed to increment download when obtaining version for Plugin '{Username}/{Name}:{Version}'",
              username, name, version);
          }

          return r;
        });
    }

    return await this._repo
      .GetVersion(username, name, version);
  }

  public async Task<Result<PluginVersion?>> GetVersion(string username, string name, bool bumpDownload)
  {
    if (bumpDownload)
    {
      return await this._repo.GetVersion(username, name)
        .DoAwait(DoType.Ignore, async _ =>
        {
          var r = await this._repo.IncrementDownload(username, name);
          if (r.IsFailure())
          {
            var err = r.FailureOrDefault();
            this._logger.LogError(err,
              "Failed to increment download when obtaining version for Plugin '{Username}/{Name}'",
              username, name);
          }

          return r;
        });
    }

    return await this._repo
      .GetVersion(username, name);
  }

  public Task<Result<PluginVersion?>> GetVersion(string userId, Guid id, ulong version)
  {
    return this._repo.GetVersion(userId, id, version);
  }

  public Task<Result<PluginVersionPrincipal?>> CreateVersion(string userId, string name, PluginVersionRecord record,
    PluginVersionProperty property)
  {
    return this._repo.CreateVersion(userId, name, record, property);
  }

  public async Task<Result<PluginVersionPrincipal?>> Push(string username, PluginRecord pRecord,
    PluginMetadata metadata, PluginVersionRecord record,
    PluginVersionProperty property)
  {
    return await this._repo.Get(username, pRecord.Name)
      .ThenAwait(async p =>
      {
        if (p != null) return p.Principal.ToResult();
        return await this._user.GetByUsername(username)
          .ThenAwait(u => this._repo.Create(u!.Principal.Id, pRecord, metadata));
      })
      .ThenAwait(x => this._repo.CreateVersion(username, pRecord.Name, record, property));
  }

  public Task<Result<PluginVersionPrincipal?>> CreateVersion(string userId, Guid id, PluginVersionRecord record,
    PluginVersionProperty property)
  {
    return this._repo.CreateVersion(userId, id, record, property);
  }

  public Task<Result<PluginVersionPrincipal?>> UpdateVersion(string userId, Guid id, ulong version,
    PluginVersionRecord record)
  {
    return this._repo.UpdateVersion(userId, id, version, record);
  }

  public Task<Result<PluginVersionPrincipal?>> UpdateVersion(string userId, string name, ulong version,
    PluginVersionRecord record)
  {
    return this._repo.UpdateVersion(userId, name, version, record);
  }
}
