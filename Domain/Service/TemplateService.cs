using CSharp_Result;
using Domain.Model;
using Domain.Repository;
using Microsoft.Extensions.Logging;

namespace Domain.Service;

public class TemplateService(
  ITemplateRepository repo,
  IPluginRepository plugin,
  IProcessorRepository processor,
  IResolverRepository resolver,
  ILogger<TemplateService> logger,
  IUserRepository user
) : ITemplateService
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

  public Task<Result<TemplatePrincipal>> Create(
    string userId,
    TemplateRecord record,
    TemplateMetadata metadata
  )
  {
    return repo.Create(userId, record, metadata);
  }

  public Task<Result<TemplatePrincipal?>> Update(string userId, Guid id, TemplateMetadata metadata)
  {
    return repo.Update(userId, id, metadata);
  }

  public Task<Result<TemplatePrincipal?>> Update(
    string userId,
    string name,
    TemplateMetadata metadata
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

  public Task<Result<IEnumerable<TemplateVersionPrincipal>>> SearchVersion(
    string userId,
    string name,
    TemplateVersionSearch version
  )
  {
    return repo.SearchVersion(userId, name, version);
  }

  public Task<Result<IEnumerable<TemplateVersionPrincipal>>> SearchVersion(
    string userId,
    Guid id,
    TemplateVersionSearch version
  )
  {
    return repo.SearchVersion(userId, id, version);
  }

  public async Task<Result<TemplateVersion?>> GetVersion(
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
                "Failed to increment download when obtaining version for Template '{Username}/{Name}:{Version}'",
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

  public async Task<Result<TemplateVersion?>> GetVersion(
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
                "Failed to increment download when obtaining version for Template '{Username}/{Name}'",
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

  public Task<Result<TemplateVersion?>> GetVersion(string userId, Guid id, ulong version)
  {
    return repo.GetVersion(userId, id, version);
  }

  public Task<Result<TemplateVersion?>> GetVersionById(Guid versionId)
  {
    return repo.GetVersionById(versionId);
  }

  public async Task<Result<TemplateVersionPrincipal?>> CreateVersion(
    string userId,
    string name,
    TemplateVersionRecord record,
    TemplateVersionProperty? property,
    string[] commands,
    IEnumerable<ProcessorVersionRef> processors,
    IEnumerable<PluginVersionRef> plugins,
    IEnumerable<TemplateVersionTemplateInput> templates,
    IEnumerable<TemplateVersionResolverInput> resolvers
  )
  {
    var resolverInputList = resolvers as TemplateVersionResolverInput[] ?? resolvers.ToArray();
    var templateInputList = templates as TemplateVersionTemplateInput[] ?? templates.ToArray();

    var pluginResults = await plugin.GetAllVersion(plugins);
    var processorResults = await processor.GetAllVersion(processors);
    var templateResults = await repo.GetAllVersion(templateInputList.Select(t => t.Template));

    // Extract refs from resolver inputs and resolve them
    var resolverRefs = resolverInputList.Select(r => r.Resolver);
    var resolverResults = await resolver.GetAllVersion(resolverRefs);

    var a =
      from plugin in pluginResults
      from processor in processorResults
      from template in templateResults
      from resolvedResolvers in resolverResults
      select (
        plugin.Select(x => x.Id),
        processor.Select(x => x.Id),
        CreateTemplateLinks(templateInputList, template),
        // Match resolved resolvers back to inputs by Username/Name to preserve Config/Files
        CreateResolverLinks(resolverInputList, resolvedResolvers)
      );
    return await Task.FromResult(a)
      .ThenAwait(refs =>
      {
        var (pl, pr, t, r) = refs;
        logger.LogInformation(
          "Creating Template Version '{Name}' for '{UserId}', Processors: {@Processors}",
          name,
          userId,
          processors
        );
        logger.LogInformation(
          "Creating Template Version '{Name}' for '{UserId}', Plugins: {@Plugins}",
          name,
          userId,
          pl
        );
        logger.LogInformation(
          "Creating Template Version '{Name}' for '{UserId}', Resolvers: {@Resolvers}",
          name,
          userId,
          r.Select(x => x.ResolverId)
        );
        return repo.CreateVersion(userId, name, record, property, commands, pr, pl, t, r);
      });
  }

  public async Task<Result<TemplateVersionPrincipal?>> CreateVersion(
    string userId,
    Guid id,
    TemplateVersionRecord record,
    TemplateVersionProperty? property,
    string[] commands,
    IEnumerable<ProcessorVersionRef> processors,
    IEnumerable<PluginVersionRef> plugins,
    IEnumerable<TemplateVersionTemplateInput> templates,
    IEnumerable<TemplateVersionResolverInput> resolvers
  )
  {
    var resolverInputList = resolvers as TemplateVersionResolverInput[] ?? resolvers.ToArray();
    var templateInputList = templates as TemplateVersionTemplateInput[] ?? templates.ToArray();

    var pluginResults = await plugin.GetAllVersion(plugins);
    var processorResults = await processor.GetAllVersion(processors);
    var templateResults = await repo.GetAllVersion(templateInputList.Select(t => t.Template));

    // Extract refs from resolver inputs and resolve them
    var resolverRefs = resolverInputList.Select(r => r.Resolver);
    var resolverResults = await resolver.GetAllVersion(resolverRefs);

    var a =
      from plugin in pluginResults
      from processor in processorResults
      from template in templateResults
      from resolvedResolvers in resolverResults
      select (
        plugin.Select(x => x.Id),
        processor.Select(x => x.Id),
        CreateTemplateLinks(templateInputList, template),
        // Match resolved resolvers back to inputs by Username/Name to preserve Config/Files
        CreateResolverLinks(resolverInputList, resolvedResolvers)
      );
    return await Task.FromResult(a)
      .ThenAwait(refs =>
      {
        var (pl, pr, t, r) = refs;
        return repo.CreateVersion(userId, id, record, property, commands, pr, pl, t, r);
      });
  }

  public Task<Result<TemplateVersionPrincipal?>> UpdateVersion(
    string userId,
    Guid id,
    ulong version,
    TemplateVersionRecord record
  )
  {
    return repo.UpdateVersion(userId, id, version, record);
  }

  public Task<Result<TemplateVersionPrincipal?>> UpdateVersion(
    string userId,
    string name,
    ulong version,
    TemplateVersionRecord record
  )
  {
    return repo.UpdateVersion(userId, name, version, record);
  }

  public async Task<Result<TemplateVersionPrincipal?>> Push(
    string username,
    TemplateRecord pRecord,
    TemplateMetadata metadata,
    TemplateVersionRecord record,
    TemplateVersionProperty? property,
    string[] commands,
    IEnumerable<ProcessorVersionRef> processors,
    IEnumerable<PluginVersionRef> plugins,
    IEnumerable<TemplateVersionTemplateInput> templates,
    IEnumerable<TemplateVersionResolverInput> resolvers
  )
  {
    return await repo.Get(username, pRecord.Name)
      .ThenAwait(async p =>
      {
        if (p != null)
          return await repo.Update(username, pRecord.Name, metadata);
        return await user.GetByUsername(username)
          .ThenAwait(u => repo.Create(u!.Principal.Id, pRecord, metadata));
      })
      .ThenAwait(x =>
        this.CreateVersion(
          username,
          pRecord.Name,
          record,
          property,
          commands,
          processors,
          plugins,
          templates,
          resolvers
        )
      );
  }

  /// <summary>
  /// Creates ResolverLink records by matching resolved resolver versions back to their original inputs.
  /// This preserves the Config and Files data through the resolution process.
  /// </summary>
  /// <param name="inputs">The original resolver inputs with Config and Files</param>
  /// <param name="resolvedResolvers">The resolved resolver versions with identity info</param>
  /// <returns>A collection of ResolverLink records ready for storage</returns>
  private static ResolverLink[] CreateResolverLinks(
    TemplateVersionResolverInput[] inputs,
    IEnumerable<ResolverVersionWithIdentity> resolvedResolvers
  )
  {
    // Match by position (index) to support duplicate resolvers with different configs
    return inputs
      .Zip(
        resolvedResolvers,
        (input, resolved) => new ResolverLink(resolved.Principal.Id, input.Config, input.Files)
      )
      .ToArray();
  }

  /// <summary>
  /// Creates TemplateLink records by matching resolved sub-template versions back to their original inputs.
  /// This preserves the PresetAnswers data through the resolution process.
  /// </summary>
  /// <param name="inputs">The original sub-template inputs with PresetAnswers</param>
  /// <param name="resolvedTemplates">The resolved sub-template versions</param>
  /// <returns>A collection of TemplateLink records ready for storage</returns>
  private static TemplateLink[] CreateTemplateLinks(
    TemplateVersionTemplateInput[] inputs,
    IEnumerable<TemplateVersionPrincipal> resolvedTemplates
  )
  {
    // Match by position (index) to support duplicate sub-templates with different preset answers
    return inputs
      .Zip(
        resolvedTemplates,
        (input, resolved) => new TemplateLink(resolved.Id, input.PresetAnswers)
      )
      .ToArray();
  }
}
