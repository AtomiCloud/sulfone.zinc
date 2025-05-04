using System.Collections;
using CSharp_Result;
using Domain.Model;

namespace Domain.Repository;

public interface IPluginRepository
{
  Task<Result<IEnumerable<PluginPrincipal>>> Search(PluginSearch search);

  Task<Result<Plugin?>> Get(string userId, Guid id);

  Task<Result<Plugin?>> Get(string username, string name);

  Task<Result<PluginPrincipal>> Create(string userId, PluginRecord record, PluginMetadata metadata);

  Task<Result<PluginPrincipal?>> Update(string userId, Guid id, PluginMetadata metadata);

  Task<Result<PluginPrincipal?>> Update(string username, string name, PluginMetadata metadata);

  Task<Result<Unit?>> Delete(string userId, Guid id);

  Task<Result<Unit?>> Like(string likerId, string username, string name, bool like);

  Task<Result<uint?>> IncrementDownload(string username, string name);

  Task<Result<IEnumerable<PluginVersionPrincipal>>> SearchVersion(
    string username,
    string name,
    PluginVersionSearch version
  );

  Task<Result<IEnumerable<PluginVersionPrincipal>>> SearchVersion(
    string userId,
    Guid id,
    PluginVersionSearch version
  );

  Task<Result<IEnumerable<PluginVersionPrincipal>>> GetAllVersion(
    IEnumerable<PluginVersionRef> references
  );

  Task<Result<PluginVersion?>> GetVersion(string username, string name, ulong version);

  Task<Result<PluginVersion?>> GetVersion(string username, string name);

  Task<Result<PluginVersion?>> GetVersion(string userId, Guid id, ulong version);

  Task<Result<PluginVersionPrincipal?>> CreateVersion(
    string username,
    string name,
    PluginVersionRecord record,
    PluginVersionProperty property
  );

  Task<Result<PluginVersionPrincipal?>> CreateVersion(
    string userId,
    Guid id,
    PluginVersionRecord record,
    PluginVersionProperty property
  );

  Task<Result<PluginVersionPrincipal?>> UpdateVersion(
    string username,
    string name,
    ulong version,
    PluginVersionRecord record
  );

  Task<Result<PluginVersionPrincipal?>> UpdateVersion(
    string userId,
    Guid id,
    ulong version,
    PluginVersionRecord record
  );
}
