using CSharp_Result;
using Domain.Model;

namespace Domain.Repository;

public interface IProcessorRepository
{
  Task<Result<IEnumerable<ProcessorPrincipal>>> Search(ProcessorSearch search);

  Task<Result<Processor?>> Get(Guid id);

  Task<Result<Processor?>> Get(string username, string name);

  Task<Result<ProcessorPrincipal>> Create(string userId, ProcessorRecord record, ProcessorMetadata metadata);

  Task<Result<ProcessorPrincipal?>> Update(string userId, Guid id, ProcessorMetadata metadata);

  Task<Result<ProcessorPrincipal?>> Update(string username, string name, ProcessorMetadata metadata);

  Task<Result<Unit?>> Delete(string userId, Guid id);

  Task<Result<Unit?>> Like(string likerId, string username, string name, bool like);

  Task<Result<uint?>> IncrementDownload(string username, string name);

  Task<Result<IEnumerable<ProcessorVersionPrincipal>>> GetAllVersion(IEnumerable<ProcessorVersionRef> references);

  Task<Result<IEnumerable<ProcessorVersionPrincipal>>> SearchVersion(string userId, string name, ProcessorVersionSearch version);

  Task<Result<IEnumerable<ProcessorVersionPrincipal>>> SearchVersion(string userId, Guid id, ProcessorVersionSearch version);

  Task<Result<ProcessorVersionPrincipal?>> GetVersion(string userId, string name, ulong version);

  Task<Result<ProcessorVersionPrincipal?>> GetVersion(string userId, Guid id, ulong version);

  Task<Result<ProcessorVersionPrincipal?>> CreateVersion(string userId, string name, ProcessorVersionRecord record,
    ProcessorVersionProperty property);

  Task<Result<ProcessorVersionPrincipal?>> CreateVersion(string userId, Guid id, ProcessorVersionRecord record,
    ProcessorVersionProperty property);

  Task<Result<ProcessorVersionPrincipal?>> UpdateVersion(string userId, Guid id, ulong version, ProcessorVersionRecord record);

  Task<Result<ProcessorVersionPrincipal?>> UpdateVersion(string userId, string name, ulong version,
    ProcessorVersionRecord record);
}
