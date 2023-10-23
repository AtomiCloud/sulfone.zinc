using CSharp_Result;
using Domain.Model;
using Domain.Repository;
using Microsoft.Extensions.Logging;

namespace Domain.Service;

public class ProcessorService : IProcessorService
{
  private readonly IProcessorRepository _repo;
  private readonly ILogger<ProcessorService> _logger;

  public ProcessorService(IProcessorRepository repo, ILogger<ProcessorService> logger)
  {
    this._repo = repo;
    this._logger = logger;
  }

  public Task<Result<IEnumerable<ProcessorPrincipal>>> Search(ProcessorSearch search)
  {
    return this._repo.Search(search);
  }

  public Task<Result<Processor?>> Get(Guid id)
  {
    return this._repo.Get(id);
  }

  public Task<Result<Processor?>> Get(string owner, string name)
  {
    return this._repo.Get(owner, name);
  }

  public Task<Result<ProcessorPrincipal>> Create(string userId, ProcessorRecord record, ProcessorMetadata metadata)
  {
    return this._repo.Create(userId, record, metadata);
  }

  public Task<Result<ProcessorPrincipal?>> Update(string userId, Guid id, ProcessorMetadata metadata)
  {
    return this._repo.Update(userId, id, metadata);
  }

  public Task<Result<ProcessorPrincipal?>> Update(string userId, string name, ProcessorMetadata metadata)
  {
    return this._repo.Update(userId, name, metadata);
  }

  public Task<Result<Unit?>> Delete(string userId, Guid id)
  {
    return this._repo.Delete(userId, id);
  }

  public Task<Result<Unit?>> Like(string likerId, string username, string name, bool like)
  {
    return this._repo.Like(likerId, username, name, like);
  }

  public Task<Result<IEnumerable<ProcessorVersionPrincipal>>> SearchVersion(string username, string name, ProcessorVersionSearch version)
  {
    return this._repo.SearchVersion(username, name, version);
  }

  public Task<Result<IEnumerable<ProcessorVersionPrincipal>>> SearchVersion(string userId, Guid id, ProcessorVersionSearch version)
  {
    return this._repo.SearchVersion(userId, id, version);
  }

  public async Task<Result<ProcessorVersionPrincipal?>> GetVersion(string username, string name, ulong version, bool bumpDownload)
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
            this._logger.LogError(err, "Failed to increment download when obtaining version for Processor '{Username}/{Name}:{Version}'",
              username, name, version);
          }

          return r;
        });
    }
    return await this._repo.GetVersion(username, name, version);
  }

  public Task<Result<ProcessorVersionPrincipal?>> GetVersion(string userId, Guid id, ulong version)
  {
    return this._repo.GetVersion(userId, id, version);
  }

  public Task<Result<ProcessorVersionPrincipal?>> CreateVersion(string username, string name, ProcessorVersionRecord record, ProcessorVersionProperty property)
  {
    return this._repo.CreateVersion(username, name, record, property);
  }

  public Task<Result<ProcessorVersionPrincipal?>> CreateVersion(string userId, Guid id, ProcessorVersionRecord record, ProcessorVersionProperty property)
  {
    return this._repo.CreateVersion(userId, id, record, property);
  }

  public Task<Result<ProcessorVersionPrincipal?>> UpdateVersion(string userId, Guid id, ulong version, ProcessorVersionRecord record)
  {
    return this._repo.UpdateVersion(userId, id, version, record);
  }

  public Task<Result<ProcessorVersionPrincipal?>> UpdateVersion(string username, string name, ulong version, ProcessorVersionRecord record)
  {
    return this._repo.UpdateVersion(username, name, version, record);
  }
}
