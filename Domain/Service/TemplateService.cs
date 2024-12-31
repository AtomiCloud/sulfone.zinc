using CSharp_Result;
using Domain.Model;
using Domain.Repository;
using Microsoft.Extensions.Logging;

namespace Domain.Service;

public class TemplateService(
  ITemplateRepository repo,
  IPluginRepository plugin,
  IProcessorRepository processor,
  ILogger<TemplateService> logger,
  IUserRepository user)
  : ITemplateService
{
  public Task<Result<IEnumerable<TemplatePrincipal>>> Search(TemplateSearch search)
  {
    return repo.Search(search);
  }

  public Task<Result<Template?>> Get(string userId, Guid id)
  {
    return repo.Get(userId, id);
  }

  public Task<Result<Template?>> Get(string owner, string name)
  {
    return repo.Get(owner, name);
  }

  public Task<Result<TemplatePrincipal>> Create(string userId, TemplateRecord record, TemplateMetadata metadata)
  {
    return repo.Create(userId, record, metadata);
  }

  public Task<Result<TemplatePrincipal?>> Update(string userId, Guid id, TemplateMetadata metadata)
  {
    return repo.Update(userId, id, metadata);
  }

  public Task<Result<TemplatePrincipal?>> Update(string userId, string name, TemplateMetadata metadata)
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

  public Task<Result<IEnumerable<TemplateVersionPrincipal>>> SearchVersion(string userId, string name,
    TemplateVersionSearch version)
  {
    return repo.SearchVersion(userId, name, version);
  }

  public Task<Result<IEnumerable<TemplateVersionPrincipal>>> SearchVersion(string userId, Guid id,
    TemplateVersionSearch version)
  {
    return repo.SearchVersion(userId, id, version);
  }

  public async Task<Result<TemplateVersion?>> GetVersion(string username, string name, ulong version,
    bool bumpDownload)
  {
    if (bumpDownload)
    {
      return await repo.GetVersion(username, name, version)
        .DoAwait(DoType.Ignore, async _ =>
        {
          var r = await repo.IncrementDownload(username, name);
          if (r.IsFailure())
          {
            var err = r.FailureOrDefault();
            logger.LogError(err,
              "Failed to increment download when obtaining version for Template '{Username}/{Name}:{Version}'",
              username, name, version);
          }

          return r;
        });
    }

    return await repo.GetVersion(username, name, version);
  }

  public async Task<Result<TemplateVersion?>> GetVersion(string username, string name, bool bumpDownload)
  {
    if (bumpDownload)
    {
      return await repo.GetVersion(username, name)
        .DoAwait(DoType.Ignore, async _ =>
        {
          var r = await repo.IncrementDownload(username, name);
          if (r.IsFailure())
          {
            var err = r.FailureOrDefault();
            logger.LogError(err,
              "Failed to increment download when obtaining version for Template '{Username}/{Name}'", username, name);
          }

          return r;
        });
    }

    return await repo.GetVersion(username, name);
  }

  public Task<Result<TemplateVersion?>> GetVersion(string userId, Guid id, ulong version)
  {
    return repo.GetVersion(userId, id, version);
  }

  public async Task<Result<TemplateVersionPrincipal?>> CreateVersion(string userId, string name,
    TemplateVersionRecord record, TemplateVersionProperty property,
    IEnumerable<ProcessorVersionRef> processors, IEnumerable<PluginVersionRef> plugins)
  {
    var pluginResults = await plugin.GetAllVersion(plugins);
    var processorResults = await processor.GetAllVersion(processors);

    var a = from plugin in pluginResults
            from processor in processorResults
            select (plugin.Select(x => x.Id), processor.Select(x => x.Id));
    return await Task.FromResult(a)
      .ThenAwait(refs =>
      {
        var (pl, pr) = refs;
        logger.LogInformation("Creating Template Version '{Name}' for '{UserId}', Processors: {@Processors}",
          name, userId, processors);
        logger.LogInformation("Creating Template Version '{Name}' for '{UserId}', Plugins: {@Plugins}", name,
          userId, pl);
        return repo.CreateVersion(userId, name, record, property, pr, pl);
      });
  }

  public async Task<Result<TemplateVersionPrincipal?>> CreateVersion(string userId, Guid id,
    TemplateVersionRecord record,
    TemplateVersionProperty property,
    IEnumerable<ProcessorVersionRef> processors, IEnumerable<PluginVersionRef> plugins)
  {
    var pluginResults = await plugin.GetAllVersion(plugins);
    var processorResults = await processor.GetAllVersion(processors);

    var a = from plugin in pluginResults
            from processor in processorResults
            select (plugin.Select(x => x.Id), processor.Select(x => x.Id));
    return await Task.FromResult(a)
      .ThenAwait(refs =>
      {
        var (pr, pl) = refs;
        return repo.CreateVersion(userId, id, record, property, pr, pl);
      });
  }

  public Task<Result<TemplateVersionPrincipal?>> UpdateVersion(string userId, Guid id, ulong version,
    TemplateVersionRecord record)
  {
    return repo.UpdateVersion(userId, id, version, record);
  }

  public Task<Result<TemplateVersionPrincipal?>> UpdateVersion(string userId, string name, ulong version,
    TemplateVersionRecord record)
  {
    return repo.UpdateVersion(userId, name, version, record);
  }

  public async Task<Result<TemplateVersionPrincipal?>> Push(string username, TemplateRecord pRecord,
    TemplateMetadata metadata, TemplateVersionRecord record,
    TemplateVersionProperty property,
    IEnumerable<ProcessorVersionRef> processors, IEnumerable<PluginVersionRef> plugins)
  {
    return await repo.Get(username, pRecord.Name)
      .ThenAwait(async p =>
      {
        if (p != null) return p.Principal.ToResult();
        return await user.GetByUsername(username)
          .ThenAwait(u => repo.Create(u!.Principal.Id, pRecord, metadata));
      })
      .ThenAwait(x => this.CreateVersion(username, pRecord.Name, record, property, processors, plugins));
  }
}
