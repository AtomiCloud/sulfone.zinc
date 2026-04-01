using System.Text.Json;
using CSharp_Result;
using Domain.Model;

namespace Domain.Repository;

/// <summary>
/// Represents a resolved link between a template version and a resolver version,
/// including the associated config and file patterns.
/// This is used when passing resolver data to the repository layer.
/// </summary>
/// <param name="ResolverId">The resolved resolver version's GUID</param>
/// <param name="Config">Dynamic JSON configuration object for the resolver</param>
/// <param name="Files">Glob patterns for file matching</param>
public record ResolverLink(Guid ResolverId, JsonElement Config, string[] Files);

/// <summary>
/// Represents a resolved link between a template version and a sub-template version,
/// including the associated preset answer configurations.
/// This is used when passing sub-template data to the repository layer.
/// </summary>
/// <param name="TemplateId">The resolved sub-template version's GUID</param>
/// <param name="PresetAnswers">Dynamic JSON preset answer configuration for the sub-template</param>
public record TemplateLink(Guid TemplateId, JsonElement PresetAnswers);

public interface ITemplateRepository
{
  Task<Result<IEnumerable<TemplatePrincipal>>> Search(TemplateSearch search);

  Task<Result<Template?>> Get(string userId, Guid id);

  Task<Result<Template?>> Get(string username, string name);

  Task<Result<TemplatePrincipal>> Create(
    string userId,
    TemplateRecord record,
    TemplateMetadata metadata
  );

  Task<Result<TemplatePrincipal?>> Update(string userId, Guid id, TemplateMetadata metadata);

  Task<Result<TemplatePrincipal?>> Update(string username, string name, TemplateMetadata metadata);

  Task<Result<Unit?>> Delete(string userId, Guid id);

  Task<Result<Unit?>> Like(string likerId, string username, string name, bool like);

  Task<Result<uint?>> IncrementDownload(string username, string name);

  Task<Result<IEnumerable<TemplateVersionPrincipal>>> SearchVersion(
    string userId,
    string name,
    TemplateVersionSearch version
  );

  Task<Result<IEnumerable<TemplateVersionPrincipal>>> SearchVersion(
    string userId,
    Guid id,
    TemplateVersionSearch version
  );

  Task<Result<IEnumerable<TemplateVersionPrincipal>>> GetAllVersion(
    IEnumerable<TemplateVersionRef> references
  );

  Task<Result<TemplateVersion?>> GetVersion(string userId, string name, ulong version);

  Task<Result<TemplateVersion?>> GetVersion(string userId, Guid id, ulong version);
  Task<Result<TemplateVersion?>> GetVersion(string username, string name);

  Task<Result<TemplateVersion?>> GetVersionById(Guid versionId);

  Task<Result<TemplateVersionPrincipal?>> CreateVersion(
    string userId,
    string name,
    TemplateVersionRecord record,
    TemplateVersionProperty? property,
    string[] commands,
    IEnumerable<Guid> processors,
    IEnumerable<Guid> plugins,
    IEnumerable<TemplateLink> templates,
    IEnumerable<ResolverLink> resolvers
  );

  Task<Result<TemplateVersionPrincipal?>> CreateVersion(
    string userId,
    Guid id,
    TemplateVersionRecord record,
    TemplateVersionProperty? property,
    string[] commands,
    IEnumerable<Guid> processors,
    IEnumerable<Guid> plugins,
    IEnumerable<TemplateLink> templates,
    IEnumerable<ResolverLink> resolvers
  );

  Task<Result<TemplateVersionPrincipal?>> UpdateVersion(
    string userId,
    Guid id,
    ulong version,
    TemplateVersionRecord record
  );

  Task<Result<TemplateVersionPrincipal?>> UpdateVersion(
    string userId,
    string name,
    ulong version,
    TemplateVersionRecord record
  );

  Task<Result<TemplateVersionPrincipal?>> UpdateAndCreateVersion(
    string username,
    string name,
    TemplateMetadata metadata,
    TemplateVersionRecord record,
    TemplateVersionProperty? property,
    string[] commands,
    IEnumerable<Guid> processors,
    IEnumerable<Guid> plugins,
    IEnumerable<TemplateLink> templates,
    IEnumerable<ResolverLink> resolvers
  );
}
