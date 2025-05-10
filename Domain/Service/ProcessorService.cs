using CSharp_Result;
using Domain.Model;
using Domain.Repository;
using Microsoft.Extensions.Logging;

namespace Domain.Service;

public class ProcessorService(
  IProcessorRepository repo,
  ILogger<ProcessorService> logger,
  IUserRepository user
) : IProcessorService
{
  public Task<Result<IEnumerable<ProcessorPrincipal>>> Search(ProcessorSearch search)
  {
    return repo.Search(search);
  }

  public Task<Result<Processor?>> Get(string userId, Guid id)
  {
    return repo.Get(userId, id);
  }

  public Task<Result<Processor?>> Get(string owner, string name)
  {
    return repo.Get(owner, name);
  }

  public Task<Result<ProcessorPrincipal>> Create(
    string userId,
    ProcessorRecord record,
    ProcessorMetadata metadata
  )
  {
    return repo.Create(userId, record, metadata);
  }

  public Task<Result<ProcessorPrincipal?>> Update(
    string userId,
    Guid id,
    ProcessorMetadata metadata
  )
  {
    return repo.Update(userId, id, metadata);
  }

  public Task<Result<ProcessorPrincipal?>> Update(
    string userId,
    string name,
    ProcessorMetadata metadata
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

  public Task<Result<IEnumerable<ProcessorVersionPrincipal>>> SearchVersion(
    string username,
    string name,
    ProcessorVersionSearch version
  )
  {
    return repo.SearchVersion(username, name, version);
  }

  public Task<Result<IEnumerable<ProcessorVersionPrincipal>>> SearchVersion(
    string userId,
    Guid id,
    ProcessorVersionSearch version
  )
  {
    return repo.SearchVersion(userId, id, version);
  }

  public async Task<Result<ProcessorVersion?>> GetVersion(
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
                "Failed to increment download when obtaining version for Processor '{Username}/{Name}:{Version}'",
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

  public async Task<Result<ProcessorVersion?>> GetVersion(
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
                "Failed to increment download when obtaining version for Processor '{Username}/{Name}'",
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

  public Task<Result<ProcessorVersion?>> GetVersion(string userId, Guid id, ulong version)
  {
    return repo.GetVersion(userId, id, version);
  }

  public Task<Result<ProcessorVersion?>> GetVersionById(Guid versionId)
  {
    return repo.GetVersionById(versionId);
  }

  public Task<Result<ProcessorVersionPrincipal?>> CreateVersion(
    string username,
    string name,
    ProcessorVersionRecord record,
    ProcessorVersionProperty property
  )
  {
    return repo.CreateVersion(username, name, record, property);
  }

  public Task<Result<ProcessorVersionPrincipal?>> CreateVersion(
    string userId,
    Guid id,
    ProcessorVersionRecord record,
    ProcessorVersionProperty property
  )
  {
    return repo.CreateVersion(userId, id, record, property);
  }

  public Task<Result<ProcessorVersionPrincipal?>> UpdateVersion(
    string userId,
    Guid id,
    ulong version,
    ProcessorVersionRecord record
  )
  {
    return repo.UpdateVersion(userId, id, version, record);
  }

  public Task<Result<ProcessorVersionPrincipal?>> UpdateVersion(
    string username,
    string name,
    ulong version,
    ProcessorVersionRecord record
  )
  {
    return repo.UpdateVersion(username, name, version, record);
  }

  public async Task<Result<ProcessorVersionPrincipal?>> Push(
    string username,
    ProcessorRecord pRecord,
    ProcessorMetadata metadata,
    ProcessorVersionRecord record,
    ProcessorVersionProperty property
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
}
