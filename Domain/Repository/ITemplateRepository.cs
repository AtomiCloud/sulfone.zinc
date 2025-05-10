using CSharp_Result;
using Domain.Model;

namespace Domain.Repository;

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

  Task<Result<TemplateVersionPrincipal?>> CreateVersion(
    string userId,
    string name,
    TemplateVersionRecord record,
    TemplateVersionProperty? property,
    IEnumerable<Guid> processors,
    IEnumerable<Guid> plugins,
    IEnumerable<Guid> templates
  );

  Task<Result<TemplateVersionPrincipal?>> CreateVersion(
    string userId,
    Guid id,
    TemplateVersionRecord record,
    TemplateVersionProperty? property,
    IEnumerable<Guid> processors,
    IEnumerable<Guid> plugins,
    IEnumerable<Guid> templates
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
}
