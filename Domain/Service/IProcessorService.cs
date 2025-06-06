using CSharp_Result;
using Domain.Model;

namespace Domain.Service;

public interface IProcessorService
{
  Task<Result<IEnumerable<ProcessorPrincipal>>> Search(ProcessorSearch search);

  Task<Result<Processor?>> Get(string userId, Guid id);

  Task<Result<Processor?>> Get(string username, string name);

  Task<Result<ProcessorPrincipal>> Create(
    string userId,
    ProcessorRecord record,
    ProcessorMetadata metadata
  );

  Task<Result<ProcessorPrincipal?>> Update(string userId, Guid id, ProcessorMetadata metadata);

  Task<Result<ProcessorPrincipal?>> Update(
    string username,
    string name,
    ProcessorMetadata metadata
  );

  Task<Result<Unit?>> Delete(string userId, Guid id);

  Task<Result<Unit?>> Like(string likerId, string username, string name, bool like);

  Task<Result<IEnumerable<ProcessorVersionPrincipal>>> SearchVersion(
    string username,
    string name,
    ProcessorVersionSearch version
  );

  Task<Result<IEnumerable<ProcessorVersionPrincipal>>> SearchVersion(
    string userId,
    Guid id,
    ProcessorVersionSearch version
  );

  Task<Result<ProcessorVersion?>> GetVersion(
    string username,
    string name,
    ulong version,
    bool bumpDownload
  );

  Task<Result<ProcessorVersion?>> GetVersion(string username, string name, bool bumpDownload);
  Task<Result<ProcessorVersion?>> GetVersion(string userId, Guid id, ulong version);

  Task<Result<ProcessorVersion?>> GetVersionById(Guid versionId);

  Task<Result<ProcessorVersionPrincipal?>> CreateVersion(
    string username,
    string name,
    ProcessorVersionRecord record,
    ProcessorVersionProperty property
  );

  Task<Result<ProcessorVersionPrincipal?>> CreateVersion(
    string userId,
    Guid id,
    ProcessorVersionRecord record,
    ProcessorVersionProperty property
  );

  Task<Result<ProcessorVersionPrincipal?>> UpdateVersion(
    string userId,
    Guid id,
    ulong version,
    ProcessorVersionRecord record
  );

  Task<Result<ProcessorVersionPrincipal?>> UpdateVersion(
    string username,
    string name,
    ulong version,
    ProcessorVersionRecord record
  );

  Task<Result<ProcessorVersionPrincipal?>> Push(
    string username,
    ProcessorRecord pRecord,
    ProcessorMetadata metadata,
    ProcessorVersionRecord record,
    ProcessorVersionProperty property
  );
}
