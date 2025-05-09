using CSharp_Result;
using Domain.Model;

namespace Domain.Service;

public interface ITemplateService
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

  Task<Result<IEnumerable<TemplateVersionPrincipal>>> SearchVersion(
    string username,
    string name,
    TemplateVersionSearch version
  );

  Task<Result<IEnumerable<TemplateVersionPrincipal>>> SearchVersion(
    string userId,
    Guid id,
    TemplateVersionSearch version
  );

  Task<Result<TemplateVersion?>> GetVersion(
    string username,
    string name,
    ulong version,
    bool dumpDownload
  );

  Task<Result<TemplateVersion?>> GetVersion(string username, string name, bool dumpDownload);

  Task<Result<TemplateVersion?>> GetVersion(string userId, Guid id, ulong version);

  Task<Result<TemplateVersionPrincipal?>> CreateVersion(
    string username,
    string name,
    TemplateVersionRecord record,
    TemplateVersionProperty property,
    IEnumerable<ProcessorVersionRef> processors,
    IEnumerable<PluginVersionRef> plugins,
    IEnumerable<TemplateVersionRef> templates
  );

  Task<Result<TemplateVersionPrincipal?>> CreateVersion(
    string userId,
    Guid id,
    TemplateVersionRecord record,
    TemplateVersionProperty property,
    IEnumerable<ProcessorVersionRef> processors,
    IEnumerable<PluginVersionRef> plugins,
    IEnumerable<TemplateVersionRef> templates
  );

  Task<Result<TemplateVersionPrincipal?>> UpdateVersion(
    string userId,
    Guid id,
    ulong version,
    TemplateVersionRecord record
  );

  Task<Result<TemplateVersionPrincipal?>> UpdateVersion(
    string username,
    string name,
    ulong version,
    TemplateVersionRecord record
  );

  Task<Result<TemplateVersionPrincipal?>> Push(
    string username,
    TemplateRecord pRecord,
    TemplateMetadata metadata,
    TemplateVersionRecord record,
    TemplateVersionProperty property,
    IEnumerable<ProcessorVersionRef> processors,
    IEnumerable<PluginVersionRef> plugins,
    IEnumerable<TemplateVersionRef> templates
  );
}
