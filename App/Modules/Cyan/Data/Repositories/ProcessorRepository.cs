using App.Error.V1;
using App.Modules.Cyan.Data.Mappers;
using App.Modules.Cyan.Data.Models;
using App.StartUp.Database;
using App.Utility;
using CSharp_Result;
using Domain.Error;
using Domain.Model;
using Domain.Repository;
using EntityFramework.Exceptions.Common;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NpgsqlTypes;

namespace App.Modules.Cyan.Data.Repositories;

public class ProcessorRepository : IProcessorRepository
{
  private readonly MainDbContext _db;
  private readonly ILogger<ProcessorRepository> _logger;

  public ProcessorRepository(MainDbContext db, ILogger<ProcessorRepository> logger)
  {
    this._db = db;
    this._logger = logger;
  }

  public async Task<Result<IEnumerable<ProcessorPrincipal>>> Search(ProcessorSearch search)
  {
    try
    {
      this._logger.LogInformation("Searching for processors with Search Params '{@SearchParams}'", search.ToJson());
      var processors = this._db.Processors.AsQueryable();

      if (search.Owner != null)
        processors = processors
          .Include(x => x.User)
          .Where(x => x.User.Username == search.Owner);

      if (search.Search != null)
        processors = processors
          .Include(x => x.User)
          .Where(x =>
            // Full text search
            x.SearchVector
              .Concat(
                EF.Functions.ToTsVector("english", x.User.Username)
              )
              .Concat(
                EF.Functions.ArrayToTsVector(x.Tags)
              )
              .Matches(EF.Functions.PlainToTsQuery("english", search.Search.Replace("/", " "))) ||
            EF.Functions.ILike(x.Name, $"%{search.Search}%") ||
            EF.Functions.ILike(x.User.Username, $"%{search.Search}%")
          )
          // Rank with full text search
          .OrderBy(x =>
            x.SearchVector
              .Concat(
                EF.Functions.ToTsVector(x.User.Username)
              )
              .Concat(
                EF.Functions.ArrayToTsVector(x.Tags)
              )
              .Rank(EF.Functions.PlainToTsQuery("english", search.Search.Replace("/", " "))));


      processors = processors.Skip(search.Skip).Take(search.Limit);
      var a = await processors
        .ToArrayAsync();
      return a.Select(x => x.ToPrincipal()).ToResult();
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e, "Failed getting processors with Search Params '{@SearchParams}'", search.ToJson());
      return e;
    }
  }

  public async Task<Result<Processor?>> Get(string userId, Guid id)
  {
    try
    {
      this._logger.LogInformation("Getting processor with '{ID}'", id);
      var processor = await this._db.Processors
        .Where(x => x.Id == id && x.UserId == userId)
        .Include(x => x.Likes)
        .Include(x => x.User)
        .Include(x => x.Versions)
        .ThenInclude(x => x.Templates)
        .FirstOrDefaultAsync();

      if (processor == null) return (Processor?)null;

      var info = new ProcessorInfo
      {
        Downloads = processor.Downloads,
        Dependencies = (uint)processor.Versions.Sum(x => x.Templates.Count()),
        Stars = (uint)processor.Likes.Count()
      };
      return processor?.ToDomain(info);
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e, "Failed getting processor '{ProcessorId}'", id);
      return e;
    }
  }

  public async Task<Result<Processor?>> Get(string username, string name)
  {
    try
    {
      this._logger.LogInformation("Getting processor '{Username}/{Name}'", username, name);
      var processor = await this._db.Processors
        .Include(x => x.User)
        .Where(x => x.User.Username == username && x.Name == name)
        .Include(x => x.Likes)
        .Include(x => x.User)
        .Include(x => x.Versions)
        .ThenInclude(x => x.Templates)
        .FirstOrDefaultAsync();

      if (processor == null) return (Processor?)null;

      var info = new ProcessorInfo
      {
        Downloads = processor.Downloads,
        Dependencies = (uint)processor.Versions.Sum(x => x.Templates.Count()),
        Stars = (uint)processor.Likes.Count()
      };
      return processor?.ToDomain(info);
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e, "Failed getting processor '{Username}/{Name}'", username, name);
      return e;
    }
  }

  public async Task<Result<ProcessorPrincipal>> Create(string userId, ProcessorRecord record,
    ProcessorMetadata metadata)
  {
    try
    {
      var data = new ProcessorData();
      data = data
          .HydrateData(record)
          .HydrateData(metadata) with
      {
        Downloads = 0,
        User = null!,
        UserId = userId,
      };
      this._logger.LogInformation("Creating processor {UserId} with Record {@Record} and Metadata {@Metadata}", userId,
        record.ToJson(), metadata.ToJson());

      var r = this._db.Processors.Add(data);
      await this._db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (UniqueConstraintException e)
    {
      this._logger.LogError(e,
        "Failed to create processor due to conflict: {UserId} with Record {@Record} and Metadata {@Metadata}", userId,
        record.ToJson(), metadata.ToJson());
      return new AlreadyExistException("Failed to create Processor due to conflicting with existing record", e,
        typeof(ProcessorPrincipal));
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e, "Failed updating processor {UserId} with Record {@Record} and Metadata {@Metadata}", userId,
          record.ToJson(), metadata.ToJson());
      return e;
    }
  }

  public async Task<Result<ProcessorPrincipal?>> Update(string userId, Guid id, ProcessorMetadata v2)
  {
    try
    {
      var v1 = await this._db.Processors
        .Where(x => x.UserId == userId && x.Id == id)
        .FirstOrDefaultAsync();
      if (v1 == null) return (ProcessorPrincipal?)null;

      var v3 = v1.HydrateData(v2) with { User = null!, };
      var updated = this._db.Processors.Update(v3);
      await this._db.SaveChangesAsync();
      return updated.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      this._logger.LogError(e,
        "Failed to update Processor with User ID '{UserID}' and Processor ID '{ProcessorID}': {@Record}",
        userId, id, v2.ToJson());
      return e;
    }
  }

  public async Task<Result<ProcessorPrincipal?>> Update(string username, string name, ProcessorMetadata v2)
  {
    try
    {
      var v1 = await this._db.Processors
        .Include(x => x.User)
        .Where(x => x.User.Username == username && x.Name == name)
        .FirstOrDefaultAsync();
      if (v1 == null) return (ProcessorPrincipal?)null;

      var v3 = v1.HydrateData(v2) with { User = null!, };
      var updated = this._db.Processors.Update(v3);
      await this._db.SaveChangesAsync();
      return updated.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      this._logger.LogError(e, "Failed to update Processor '{Username}/{Name}': {@Record}",
        username, name, v2.ToJson());
      return e;
    }
  }

  public async Task<Result<Unit?>> Delete(string userId, Guid id)
  {
    try
    {
      var a = await this._db.Processors
        .Where(x => x.UserId == userId && x.Id == id)
        .FirstOrDefaultAsync();
      if (a == null) return (Unit?)null;

      this._db.Processors.Remove(a);
      await this._db.SaveChangesAsync();
      return new Unit();
    }
    catch (Exception e)
    {
      this._logger.LogError(e, "Failed to delete Processor '{ProcessorId}' for User '{UserId}", id, userId);
      return e;
    }
  }

  public async Task<Result<Unit?>> Like(string likerId, string username, string name, bool like)
  {
    try
    {
      // check if like already exist
      var likeExist = await this._db.ProcessorLikes
        .Include(x => x.Processor)
        .ThenInclude(x => x.User)
        .AnyAsync(x => x.UserId == likerId
                       && x.Processor.User.Username == username
                       && x.Processor.Name == name
        );

      if (like == likeExist)
        return new LikeConflictError("Failed to like processors", $"{username}/{name}", "processor",
          like ? "like" : "unlike").ToException();

      var p = await this._db.Processors
        .Include(x => x.User)
        .FirstOrDefaultAsync(x => x.User.Username == username && x.Name == name);

      if (p == null) return (Unit?)null;

      if (like)
      {
        // if like, check for conflict
        var l = new ProcessorLikeData { UserId = likerId, User = null!, ProcessorId = p.Id, Processor = null! };
        var r = this._db.ProcessorLikes.Add(l);
        await this._db.SaveChangesAsync();
        return new Unit();
      }
      else
      {
        var l = await this._db.ProcessorLikes
          .FirstOrDefaultAsync(x => x.UserId == likerId && x.ProcessorId == p.Id);
        if (l == null)
        {
          this._logger.LogError(
            "Race Condition, Failed to unlike Processor '{Username}/{Name}' for User '{UserId}'. User-Processor-Like entry does not exist even though it exist at the start of the query",
            username, name, likerId);

          return new LikeRaceConditionError("Failed to like processors", $"{username}/{name}", "processor",
            like ? "like" : "unlike").ToException();
        }

        this._db.Remove(l with { Processor = null!, User = null! });
        await this._db.SaveChangesAsync();
        return new Unit();
      }
    }
    catch (Exception e)
    {
      this._logger.LogError(e, "Failed to {Like} Processor '{Username}/{Name}' for User '{UserId}",
        like ? "like" : "unlike", username, name, likerId);
      return e;
    }
  }

  public async Task<Result<uint?>> IncrementDownload(string username, string name)
  {
    try
    {
      var processor = await this._db.Processors
        .Include(x => x.User)
        .Where(x => x.User.Username == username && x.Name == name)
        .FirstOrDefaultAsync();
      if (processor == null) return (uint?)null;

      processor = processor with { Downloads = processor.Downloads + 1, User = null!, };

      var updated = this._db.Processors.Update(processor);
      await this._db.SaveChangesAsync();
      return updated.Entity.Downloads;
    }
    catch (Exception e)
    {
      this._logger.LogError(e, "Failed to increment download count for Processor '{Username}/{Name}'",
        username, name);
      return e;
    }
  }

  public async Task<Result<IEnumerable<ProcessorVersionPrincipal>>> SearchVersion(string username, string name,
    ProcessorVersionSearch version)
  {
    try
    {
      var query = this._db.ProcessorVersions
        .Include(x => x.Processor)
        .ThenInclude(x => x.User)
        .Where(x => x.Processor.User.Username == username && x.Processor.Name == name)
        .AsQueryable();

      if (version.Search != null)
        query = query.Where(x =>
          EF.Functions.ILike(x.Description, $"%{version.Search}%") ||
          version.Search.Contains(version.Search)
        );

      var processors = await query
        .Skip(version.Skip)
        .Take(version.Limit)
        .ToArrayAsync();

      return processors.Select(x => x.ToPrincipal()).ToResult();
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e, "Failed searching processor version of Processor '{Username}/{Name}' with {@Params}",
          username, name, version.ToJson());
      return e;
    }
  }

  public async Task<Result<IEnumerable<ProcessorVersionPrincipal>>> SearchVersion(string userId, Guid id,
    ProcessorVersionSearch version)
  {
    try
    {
      var query = this._db.ProcessorVersions
        .Include(x => x.Processor)
        .Where(x => x.Processor.UserId == userId && x.Processor.Id == id)
        .AsQueryable();

      if (version.Search != null)
        query = query.Where(x =>
          EF.Functions.ILike(x.Description, $"%{version.Search}%") ||
          version.Search.Contains(version.Search)
        );

      var processors = await query
        .Skip(version.Skip)
        .Take(version.Limit)
        .ToArrayAsync();

      return processors.Select(x => x.ToPrincipal()).ToResult();
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e,
          "Failed searching processor version of Processor '{ProcessorId}' of User '{UserId}' with {@Params}",
          id, userId, version.ToJson());
      return e;
    }
  }

  public async Task<Result<IEnumerable<ProcessorVersionPrincipal>>> GetAllVersion(
    IEnumerable<ProcessorVersionRef> references)
  {
    var processorRefs = references as ProcessorVersionRef[] ?? references.ToArray();
    try
    {
      this._logger.LogInformation("Getting all processors versions {@Processors}", processorRefs.ToJson());
      if (processorRefs.IsNullOrEmpty()) return Array.Empty<ProcessorVersionPrincipal>();
      var query = this._db.ProcessorVersions
        .Include(x => x.Processor)
        .ThenInclude(x => x.User)
        .AsQueryable();

      var predicate = PredicateBuilder.New<ProcessorVersionData>(true);

      predicate = processorRefs.Aggregate(predicate, (c, r) =>
        r.Version != null
          ? c.Or(x => x.Processor.Name == r.Name && x.Processor.User.Username == r.Username && x.Version == r.Version)
          : c.Or(x => x.Processor.Name == r.Name && x.Processor.User.Username == r.Username)
      );

      query = query.Where(predicate)
        .GroupBy(x => new { x.Processor.Name, x.Processor.User.Username })
        .Select(g => g.OrderByDescending(o => o.Version).First());

      var processors = await query.Select(x => x.ToPrincipal()).ToArrayAsync();
      this._logger.LogInformation("Processor References: {@ProcessorReferences}", processors.Select(x => x.Id));

      if (processors.Length != processorRefs.Length)
      {
        var found = await query.Select(x => $"{x.Processor.User.Username}/{x.Processor.Name}:{x.Version}")
          .ToArrayAsync();
        var search = processorRefs.Select(x => $"{x.Username}/{x.Name}:{x.Version}");
        var notFound = search.Except(found).ToArray();
        return new MultipleEntityNotFound("Processors not found", typeof(ProcessorPrincipal), notFound, found)
          .ToException();
      }
      return processors;
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e, "Failed searching processor versions '{@References}'", processorRefs.ToJson());
      return e;
    }
  }

  public async Task<Result<ProcessorVersion?>> GetVersion(string username, string name, ulong version)
  {
    try
    {
      this._logger.LogInformation("Getting processor version '{Username}/{Name}:{Version}'", username, name, version);
      var processor = await this._db.ProcessorVersions
        .Include(x => x.Processor)
        .ThenInclude(x => x.User)
        .Where(x => x.Processor.User.Username == username && x.Processor.Name == name && x.Version == version)
        .FirstOrDefaultAsync();

      return processor?.ToDomain();
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e, "Failed to get processor version '{Username}/{Name}:{Version}'", username, name, version);
      return e;
    }
  }

  public async Task<Result<ProcessorVersion?>> GetVersion(string username, string name)
  {
    try
    {
      this._logger.LogInformation("Getting processor version '{Username}/{Name}'", username, name);
      var processor = await this._db.ProcessorVersions
        .Include(x => x.Processor)
        .ThenInclude(x => x.User)
        .Where(x => x.Processor.User.Username == username && x.Processor.Name == name)
        .OrderByDescending(x => x.Version)
        .FirstOrDefaultAsync();

      return processor?.ToDomain();
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e, "Failed to get processor version '{Username}/{Name}'", username, name);
      return e;
    }
  }

  public async Task<Result<ProcessorVersion?>> GetVersion(string userId, Guid id, ulong version)
  {
    try
    {
      this._logger.LogInformation(
        "Getting processor version for User '{UserId}', Processor: '{ProcessorId}', Version: {Version}'", userId, id,
        version);
      var processor = await this._db.ProcessorVersions
        .Include(x => x.Processor)
        .Where(x => x.Processor.UserId == userId && x.Processor.Id == id && x.Version == version)
        .FirstOrDefaultAsync();

      return processor?.ToDomain();
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e, "Failed to get processor version: User '{UserId}', Processor '{Name}', Version {Version}'", userId,
          id,
          version);
      return e;
    }
  }

  public async Task<Result<ProcessorVersionPrincipal?>> CreateVersion(string username, string name,
    ProcessorVersionRecord record,
    ProcessorVersionProperty property)
  {
    try
    {
      this._logger.LogInformation(
        "Creating processor version for '{Username}/{Name}' with Record {@Record} and Property {@Property} ",
        username, name, record.ToJson(), property.ToJson());

      var processor = await this._db.Processors
        .Include(x => x.User)
        .Where(x => x.User.Username == username && x.Name == name)
        .FirstOrDefaultAsync();

      if (processor == null) return (ProcessorVersionPrincipal?)null;

      var latest = this._db.ProcessorVersions
        .Where(x => x.ProcessorId == processor.Id)
        .Max(x => x.Version as ulong?) ?? 0;

      var data = new ProcessorVersionData();
      data = data
          .HydrateData(record)
          .HydrateData(property)
        with
      {
        ProcessorId = processor.Id,
        Processor = null!,
        Version = latest + 1,
        CreatedAt = DateTime.UtcNow,
      };

      var r = this._db.ProcessorVersions.Add(data);
      await this._db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e,
          "Failed to create processor version for '{Username}/{Name}' with Record {@Record} and Property {@Property} ",
          username, name, record.ToJson(), property.ToJson());
      return e;
    }
  }

  public async Task<Result<ProcessorVersionPrincipal?>> CreateVersion(string userId, Guid id,
    ProcessorVersionRecord record,
    ProcessorVersionProperty property)
  {
    try
    {
      this._logger.LogInformation(
        "Creating processor version for User '{UserId}', Processor '{Id}' with Record {@Record} and Property {@Property} ",
        userId, id, record.ToJson(), property.ToJson());

      var processor = await this._db.Processors
        .Where(x => x.UserId == userId && x.Id == id)
        .FirstOrDefaultAsync();

      if (processor == null) return (ProcessorVersionPrincipal?)null;

      var latest = this._db.ProcessorVersions
        .Where(x => x.ProcessorId == processor.Id)
        .Max(x => x.Version as ulong?) ?? 0;

      var data = new ProcessorVersionData();
      data = data
          .HydrateData(record)
          .HydrateData(property)
        with
      {
        ProcessorId = processor.Id,
        Processor = null!,
        Version = latest + 1,
        CreatedAt = DateTime.UtcNow,
      };

      var r = this._db.ProcessorVersions.Add(data);
      await this._db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e,
          "Failed to create processor version for User '{UserId}', Processor '{Id}' with Record {@Record} and Property {@Property}",
          userId, id, record.ToJson(), property.ToJson());
      return e;
    }
  }

  public async Task<Result<ProcessorVersionPrincipal?>> UpdateVersion(string username, string name, ulong version,
    ProcessorVersionRecord v2)
  {
    try
    {
      this._logger.LogInformation(
        "Updating processor '{Username}/{Name}:{Version}' with Record {@Record}",
        username, name, version, v2.ToJson());


      var v1 = await this._db.ProcessorVersions
        .Include(x => x.Processor)
        .ThenInclude(x => x.User)
        .Where(x => x.Version == version && x.Processor.Name == name && x.Processor.User.Username == username)
        .FirstOrDefaultAsync();

      if (v1 == null) return (ProcessorVersionPrincipal?)null;

      var v3 = v1.HydrateData(v2)
        with
      {
        Processor = null!,
      };

      var r = this._db.ProcessorVersions.Update(v3);
      await this._db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e,
          "Failed to update processor '{Username}/{Name}:{Version}' with Record {@Record}",
          username, name, version, v2.ToJson());
      return e;
    }
  }

  public async Task<Result<ProcessorVersionPrincipal?>> UpdateVersion(string userId, Guid id, ulong version,
    ProcessorVersionRecord v2)
  {
    try
    {
      this._logger.LogInformation(
        "Updating processor for User '{UserId}', Processor '{Id}' with Record {@Record}",
        userId, id, v2.ToJson());


      var v1 = await this._db.ProcessorVersions
        .Include(x => x.Processor)
        .Where(x => x.Version == version && x.Processor.Id == id && x.Processor.UserId == userId)
        .FirstOrDefaultAsync();

      if (v1 == null) return (ProcessorVersionPrincipal?)null;

      var v3 = v1.HydrateData(v2)
        with
      {
        Processor = null!,
      };

      var r = this._db.ProcessorVersions.Update(v3);
      await this._db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e,
          "Failed to update processor for User '{UserId}', Processor '{Id}' with Record {@Record}",
          userId, id, v2.ToJson());
      return e;
    }
  }
}
