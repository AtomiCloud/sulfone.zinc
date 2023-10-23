using CSharp_Result;
using Domain.Model;
using Domain.Repository;
using Microsoft.Extensions.Logging;

namespace Domain.Service;

public class TemplateService : ITemplateService
{
  private readonly ITemplateRepository _repo;
  private readonly IPluginRepository _plugin;
  private readonly IProcessorRepository _processor;
  private readonly ILogger<TemplateService> _logger;


  public TemplateService(ITemplateRepository repo, IPluginRepository plugin, IProcessorRepository processor,
    ILogger<TemplateService> logger)
  {
    this._repo = repo;
    this._plugin = plugin;
    this._processor = processor;
    this._logger = logger;
  }


  public Task<Result<IEnumerable<TemplatePrincipal>>> Search(TemplateSearch search)
  {
    return this._repo.Search(search);
  }

  public Task<Result<Template?>> Get(string userId, Guid id)
  {
    return this._repo.Get(userId, id);
  }

  public Task<Result<Template?>> Get(string owner, string name)
  {
    return this._repo.Get(owner, name);
  }

  public Task<Result<TemplatePrincipal>> Create(string userId, TemplateRecord record, TemplateMetadata metadata)
  {
    return this._repo.Create(userId, record, metadata);
  }

  public Task<Result<TemplatePrincipal?>> Update(string userId, Guid id, TemplateMetadata metadata)
  {
    return this._repo.Update(userId, id, metadata);
  }

  public Task<Result<TemplatePrincipal?>> Update(string userId, string name, TemplateMetadata metadata)
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

  public Task<Result<IEnumerable<TemplateVersionPrincipal>>> SearchVersion(string userId, string name,
    TemplateVersionSearch version)
  {
    return this._repo.SearchVersion(userId, name, version);
  }

  public Task<Result<IEnumerable<TemplateVersionPrincipal>>> SearchVersion(string userId, Guid id,
    TemplateVersionSearch version)
  {
    return this._repo.SearchVersion(userId, id, version);
  }

  public async Task<Result<TemplateVersion?>> GetVersion(string username, string name, ulong version,
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
              "Failed to increment download when obtaining version for Template '{Username}/{Name}:{Version}'",
              username, name, version);
          }

          return r;
        });
    }

    return await this._repo.GetVersion(username, name, version);
  }

  public Task<Result<TemplateVersion?>> GetVersion(string userId, Guid id, ulong version)
  {
    return this._repo.GetVersion(userId, id, version);
  }

  public async Task<Result<TemplateVersionPrincipal?>> CreateVersion(string userId, string name,
    TemplateVersionRecord record, TemplateVersionProperty property,
    IEnumerable<ProcessorVersionRef> processors, IEnumerable<PluginVersionRef> plugins)
  {
    var pluginResults = await this._plugin.GetAllVersion(plugins);
    var processorResults = await this._processor.GetAllVersion(processors);

    var a = from plugin in pluginResults
            from processor in processorResults
            select (plugin.Select(x => x.Id), processor.Select(x => x.Id));
    return await Task.FromResult(a)
      .ThenAwait(refs =>
      {
        var (pl, pr) = refs;
        this._logger.LogInformation("Creating Template Version '{Name}' for '{UserId}', Processors: {@Processors}", name, userId, processors);
        this._logger.LogInformation("Creating Template Version '{Name}' for '{UserId}', Plugins: {@Plugins}", name, userId, pl);
        return this._repo.CreateVersion(userId, name, record, property, pr, pl);
      });
  }

  public async Task<Result<TemplateVersionPrincipal?>> CreateVersion(string userId, Guid id,
    TemplateVersionRecord record,
    TemplateVersionProperty property,
    IEnumerable<ProcessorVersionRef> processors, IEnumerable<PluginVersionRef> plugins)
  {
    var pluginResults = await this._plugin.GetAllVersion(plugins);
    var processorResults = await this._processor.GetAllVersion(processors);

    var a = from plugin in pluginResults
            from processor in processorResults
            select (plugin.Select(x => x.Id), processor.Select(x => x.Id));
    return await Task.FromResult(a)
      .ThenAwait(refs =>
      {
        var (pr, pl) = refs;
        return this._repo.CreateVersion(userId, id, record, property, pr, pl);
      });
  }

  public Task<Result<TemplateVersionPrincipal?>> UpdateVersion(string userId, Guid id, ulong version,
    TemplateVersionRecord record)
  {
    return this._repo.UpdateVersion(userId, id, version, record);
  }

  public Task<Result<TemplateVersionPrincipal?>> UpdateVersion(string userId, string name, ulong version,
    TemplateVersionRecord record)
  {
    return this._repo.UpdateVersion(userId, name, version, record);
  }
}
