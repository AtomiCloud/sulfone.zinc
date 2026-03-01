using CSharp_Result;
using Domain.Model;

namespace Domain.Service;

public interface IResolverService
{
  Task<Result<IEnumerable<ResolverPrincipal>>> Search(ResolverSearch search);

  Task<Result<Resolver?>> Get(string userId, Guid id);

  Task<Result<Resolver?>> Get(string username, string name);

  Task<Result<ResolverPrincipal>> Create(
    string userId,
    ResolverRecord record,
    ResolverMetadata metadata
  );

  Task<Result<ResolverPrincipal?>> Update(string userId, Guid id, ResolverMetadata metadata);

  Task<Result<ResolverPrincipal?>> Update(string username, string name, ResolverMetadata metadata);

  Task<Result<Unit?>> Delete(string userId, Guid id);

  Task<Result<Unit?>> Like(string likerId, string username, string name, bool like);

  Task<Result<IEnumerable<ResolverVersionPrincipal>>> SearchVersion(
    string username,
    string name,
    ResolverVersionSearch version
  );

  Task<Result<IEnumerable<ResolverVersionPrincipal>>> SearchVersion(
    string userId,
    Guid id,
    ResolverVersionSearch version
  );

  Task<Result<ResolverVersion?>> GetVersion(
    string username,
    string name,
    ulong version,
    bool bumpDownload
  );

  Task<Result<ResolverVersion?>> GetVersion(string username, string name, bool bumpDownload);
  Task<Result<ResolverVersion?>> GetVersion(string userId, Guid id, ulong version);

  Task<Result<ResolverVersion?>> GetVersionById(Guid versionId);

  Task<Result<ResolverVersionPrincipal?>> CreateVersion(
    string username,
    string name,
    ResolverVersionRecord record,
    ResolverVersionProperty property
  );

  Task<Result<ResolverVersionPrincipal?>> CreateVersion(
    string userId,
    Guid id,
    ResolverVersionRecord record,
    ResolverVersionProperty property
  );

  Task<Result<ResolverVersionPrincipal?>> UpdateVersion(
    string userId,
    Guid id,
    ulong version,
    ResolverVersionRecord record
  );

  Task<Result<ResolverVersionPrincipal?>> UpdateVersion(
    string username,
    string name,
    ulong version,
    ResolverVersionRecord record
  );

  Task<Result<ResolverVersionPrincipal?>> Push(
    string username,
    ResolverRecord pRecord,
    ResolverMetadata metadata,
    ResolverVersionRecord record,
    ResolverVersionProperty property
  );
}
