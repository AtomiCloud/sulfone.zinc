using CSharp_Result;
using Domain.Model;

namespace Domain.Repository;

/// <summary>
/// A resolver version principal with its identity information (username and name).
/// Used for correlating resolved versions back to their original references.
/// </summary>
public record ResolverVersionWithIdentity(
  string Username,
  string Name,
  ResolverVersionPrincipal Principal
);

public interface IResolverRepository
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

  Task<Result<uint?>> IncrementDownload(string username, string name);

  Task<Result<IEnumerable<ResolverVersionWithIdentity>>> GetAllVersion(
    IEnumerable<ResolverVersionRef> references
  );

  Task<Result<IEnumerable<ResolverVersionPrincipal>>> SearchVersion(
    string userId,
    string name,
    ResolverVersionSearch version
  );

  Task<Result<IEnumerable<ResolverVersionPrincipal>>> SearchVersion(
    string userId,
    Guid id,
    ResolverVersionSearch version
  );

  Task<Result<ResolverVersion?>> GetVersion(string userId, string name, ulong version);

  Task<Result<ResolverVersion?>> GetVersion(string username, string name);

  Task<Result<ResolverVersion?>> GetVersion(string userId, Guid id, ulong version);

  Task<Result<ResolverVersion?>> GetVersionById(Guid versionId);

  Task<Result<ResolverVersionPrincipal?>> CreateVersion(
    string userId,
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
    string userId,
    string name,
    ulong version,
    ResolverVersionRecord record
  );

  Task<Result<ResolverVersionPrincipal?>> UpdateAndCreateVersion(
    string username,
    string name,
    ResolverMetadata metadata,
    ResolverVersionRecord record,
    ResolverVersionProperty property
  );
}
