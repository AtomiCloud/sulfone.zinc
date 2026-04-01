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

namespace App.Modules.Cyan.Data.Repositories;

public class ResolverRepository(MainDbContext db, ILogger<ResolverRepository> logger)
  : IResolverRepository
{
  public async Task<Result<IEnumerable<ResolverPrincipal>>> Search(ResolverSearch search)
  {
    try
    {
      logger.LogInformation(
        "Searching for resolvers with Search Params '{@SearchParams}'",
        search.ToJson()
      );
      var resolvers = db.Resolvers.AsQueryable();

      if (search.Owner != null)
        resolvers = resolvers.Include(x => x.User).Where(x => x.User.Username == search.Owner);

      if (search.Search != null)
        resolvers = resolvers
          .Include(x => x.User)
          .Where(x =>
            // Full text search
            x.SearchVector.Concat(EF.Functions.ToTsVector("english", x.User.Username))
              .Concat(EF.Functions.ArrayToTsVector(x.Tags))
              .Matches(EF.Functions.PlainToTsQuery("english", search.Search.Replace("/", " ")))
            || EF.Functions.ILike(x.Name, $"%{search.Search}%")
            || EF.Functions.ILike(x.User.Username, $"%{search.Search}%")
          )
          // Rank with full text search
          .OrderBy(x =>
            x.SearchVector.Concat(EF.Functions.ToTsVector(x.User.Username))
              .Concat(EF.Functions.ArrayToTsVector(x.Tags))
              .Rank(EF.Functions.PlainToTsQuery("english", search.Search.Replace("/", " ")))
          );

      resolvers = resolvers.Skip(search.Skip).Take(search.Limit);
      var a = await resolvers.ToArrayAsync();
      return a.Select(x => x.ToPrincipal()).ToResult();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed getting resolvers with Search Params '{@SearchParams}'",
        search.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<Resolver?>> Get(string userId, Guid id)
  {
    try
    {
      logger.LogInformation("Getting resolver with '{ID}'", id);
      var resolver = await db
        .Resolvers.Where(x => x.Id == id && x.UserId == userId)
        .Include(x => x.Likes)
        .Include(x => x.User)
        .Include(x => x.Versions)
        .ThenInclude(x => x.Templates)
        .FirstOrDefaultAsync();

      if (resolver == null)
        return (Resolver?)null;

      var info = new ResolverInfo
      {
        Downloads = resolver.Downloads,
        Dependencies = (uint)resolver.Versions.Sum(x => x.Templates.Count()),
        Stars = (uint)resolver.Likes.Count(),
      };
      return resolver?.ToDomain(info);
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed getting resolver '{ResolverId}'", id);
      return e;
    }
  }

  public async Task<Result<Resolver?>> Get(string username, string name)
  {
    try
    {
      logger.LogInformation("Getting resolver '{Username}/{Name}'", username, name);
      var resolver = await db
        .Resolvers.Include(x => x.User)
        .Where(x => x.User.Username == username && x.Name == name)
        .Include(x => x.Likes)
        .Include(x => x.User)
        .Include(x => x.Versions)
        .ThenInclude(x => x.Templates)
        .FirstOrDefaultAsync();

      if (resolver == null)
        return (Resolver?)null;

      var info = new ResolverInfo
      {
        Downloads = resolver.Downloads,
        Dependencies = (uint)resolver.Versions.Sum(x => x.Templates.Count()),
        Stars = (uint)resolver.Likes.Count(),
      };
      return resolver?.ToDomain(info);
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed getting resolver '{Username}/{Name}'", username, name);
      return e;
    }
  }

  public async Task<Result<ResolverPrincipal>> Create(
    string userId,
    ResolverRecord record,
    ResolverMetadata metadata
  )
  {
    try
    {
      var data = new ResolverData();
      data = data.HydrateData(record).HydrateData(metadata) with
      {
        Downloads = 0,
        User = null!,
        UserId = userId,
      };
      logger.LogInformation(
        "Creating resolver {UserId} with Record {@Record} and Metadata {@Metadata}",
        userId,
        record.ToJson(),
        metadata.ToJson()
      );

      var r = db.Resolvers.Add(data);
      await db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (UniqueConstraintException e)
    {
      logger.LogError(
        e,
        "Failed to create resolver due to conflict: {UserId} with Record {@Record} and Metadata {@Metadata}",
        userId,
        record.ToJson(),
        metadata.ToJson()
      );
      return new AlreadyExistException(
        "Failed to create Resolver due to conflicting with existing record",
        e,
        typeof(ResolverPrincipal)
      );
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed creating resolver {UserId} with Record {@Record} and Metadata {@Metadata}",
        userId,
        record.ToJson(),
        metadata.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<ResolverPrincipal?>> Update(string userId, Guid id, ResolverMetadata v2)
  {
    try
    {
      var v1 = await db
        .Resolvers.Where(x => x.UserId == userId && x.Id == id)
        .FirstOrDefaultAsync();
      if (v1 == null)
        return (ResolverPrincipal?)null;

      var v3 = v1.HydrateData(v2) with { User = null! };
      var updated = db.Resolvers.Update(v3);
      await db.SaveChangesAsync();
      return updated.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to update Resolver with User ID '{UserID}' and Resolver ID '{ResolverID}': {@Record}",
        userId,
        id,
        v2.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<ResolverPrincipal?>> Update(
    string username,
    string name,
    ResolverMetadata v2
  )
  {
    try
    {
      var v1 = await db
        .Resolvers.Include(x => x.User)
        .Where(x => x.User.Username == username && x.Name == name)
        .FirstOrDefaultAsync();
      if (v1 == null)
        return (ResolverPrincipal?)null;

      var v3 = v1.HydrateData(v2) with { User = null! };
      var updated = db.Resolvers.Update(v3);
      await db.SaveChangesAsync();
      return updated.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to update Resolver '{Username}/{Name}': {@Record}",
        username,
        name,
        v2.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<Unit?>> Delete(string userId, Guid id)
  {
    try
    {
      var a = await db.Resolvers.Where(x => x.UserId == userId && x.Id == id).FirstOrDefaultAsync();
      if (a == null)
        return (Unit?)null;

      db.Resolvers.Remove(a);
      await db.SaveChangesAsync();
      return new Unit();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to delete Resolver '{ResolverId}' for User '{UserId}", id, userId);
      return e;
    }
  }

  public async Task<Result<Unit?>> Like(string likerId, string username, string name, bool like)
  {
    try
    {
      // check if like already exist
      var likeExist = await db
        .ResolverLikes.Include(x => x.Resolver)
        .ThenInclude(x => x.User)
        .AnyAsync(x =>
          x.UserId == likerId && x.Resolver.User.Username == username && x.Resolver.Name == name
        );

      if (like == likeExist)
        return new LikeConflictError(
          "Failed to like resolvers",
          $"{username}/{name}",
          "resolver",
          like ? "like" : "unlike"
        ).ToException();

      var p = await db
        .Resolvers.Include(x => x.User)
        .FirstOrDefaultAsync(x => x.User.Username == username && x.Name == name);

      if (p == null)
        return (Unit?)null;

      if (like)
      {
        // if like, check for conflict
        var l = new ResolverLikeData
        {
          UserId = likerId,
          User = null!,
          ResolverId = p.Id,
          Resolver = null!,
        };
        var r = db.ResolverLikes.Add(l);
        await db.SaveChangesAsync();
        return new Unit();
      }
      else
      {
        var l = await db.ResolverLikes.FirstOrDefaultAsync(x =>
          x.UserId == likerId && x.ResolverId == p.Id
        );
        if (l == null)
        {
          logger.LogError(
            "Race Condition, Failed to unlike Resolver '{Username}/{Name}' for User '{UserId}'. User-Resolver-Like entry does not exist even though it exist at the start of the query",
            username,
            name,
            likerId
          );

          return new LikeRaceConditionError(
            "Failed to like resolvers",
            $"{username}/{name}",
            "resolver",
            like ? "like" : "unlike"
          ).ToException();
        }

        db.Remove(l with { Resolver = null!, User = null! });
        await db.SaveChangesAsync();
        return new Unit();
      }
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to {Like} Resolver '{Username}/{Name}' for User '{UserId}",
        like ? "like" : "unlike",
        username,
        name,
        likerId
      );
      return e;
    }
  }

  public async Task<Result<uint?>> IncrementDownload(string username, string name)
  {
    try
    {
      var resolver = await db
        .Resolvers.Include(x => x.User)
        .Where(x => x.User.Username == username && x.Name == name)
        .FirstOrDefaultAsync();
      if (resolver == null)
        return (uint?)null;

      resolver = resolver with { Downloads = resolver.Downloads + 1, User = null! };

      var updated = db.Resolvers.Update(resolver);
      await db.SaveChangesAsync();
      return updated.Entity.Downloads;
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to increment download count for Resolver '{Username}/{Name}'",
        username,
        name
      );
      return e;
    }
  }

  public async Task<Result<IEnumerable<ResolverVersionPrincipal>>> SearchVersion(
    string username,
    string name,
    ResolverVersionSearch version
  )
  {
    try
    {
      var query = db
        .ResolverVersions.Include(x => x.Resolver)
        .ThenInclude(x => x.User)
        .Where(x => x.Resolver.User.Username == username && x.Resolver.Name == name)
        .AsQueryable();

      if (version.Search != null)
        query = query.Where(x => EF.Functions.ILike(x.Description, $"%{version.Search}%"));

      var resolvers = await query.Skip(version.Skip).Take(version.Limit).ToArrayAsync();

      return resolvers.Select(x => x.ToPrincipal()).ToResult();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed searching resolver version of Resolver '{Username}/{Name}' with {@Params}",
        username,
        name,
        version.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<IEnumerable<ResolverVersionPrincipal>>> SearchVersion(
    string userId,
    Guid id,
    ResolverVersionSearch version
  )
  {
    try
    {
      var query = db
        .ResolverVersions.Include(x => x.Resolver)
        .Where(x => x.Resolver.UserId == userId && x.Resolver.Id == id)
        .AsQueryable();

      if (version.Search != null)
        query = query.Where(x => EF.Functions.ILike(x.Description, $"%{version.Search}%"));

      var resolvers = await query.Skip(version.Skip).Take(version.Limit).ToArrayAsync();

      return resolvers.Select(x => x.ToPrincipal()).ToResult();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed searching resolver version of Resolver '{ResolverId}' of User '{UserId}' with {@Params}",
        id,
        userId,
        version.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<IEnumerable<ResolverVersionWithIdentity>>> GetAllVersion(
    IEnumerable<ResolverVersionRef> references
  )
  {
    var resolverRefs = references as ResolverVersionRef[] ?? references.ToArray();
    try
    {
      logger.LogInformation("Getting all resolvers versions {@Resolvers}", resolverRefs.ToJson());
      if (resolverRefs.IsNullOrEmpty())
        return Array.Empty<ResolverVersionWithIdentity>();
      var query = db
        .ResolverVersions.Include(x => x.Resolver)
        .ThenInclude(x => x.User)
        .AsQueryable();

      var predicate = PredicateBuilder.New<ResolverVersionData>(true);

      predicate = resolverRefs.Aggregate(
        predicate,
        (c, r) =>
          r.Version != null
            ? c.Or(x =>
              x.Resolver.Name == r.Name
              && x.Resolver.User.Username == r.Username
              && x.Version == r.Version
            )
            : c.Or(x => x.Resolver.Name == r.Name && x.Resolver.User.Username == r.Username)
      );

      var all = await query.Where(predicate).ToArrayAsync();

      // Build lookup keyed by (username, name, version) for O(1) access
      var lookup = all.ToDictionary(
        x => (x.Resolver.User.Username, x.Resolver.Name, x.Version),
        x => x
      );

      // Build latest version lookup keyed by (username, name) for null-version resolution
      var latestVersionLookup = all.GroupBy(x => (x.Resolver.User.Username, x.Resolver.Name))
        .ToDictionary(g => g.Key, g => g.Max(x => x.Version));

      // Check for missing refs: null-version refs need to exist in latestVersionLookup,
      // versioned refs need to exist in lookup
      var missingRefs = resolverRefs
        .Select(r =>
        {
          var isMissing =
            r.Version == null
              ? !latestVersionLookup.ContainsKey((r.Username, r.Name))
              : !lookup.ContainsKey((r.Username, r.Name, r.Version.Value));
          return (r.Username, r.Name, r.Version, IsMissing: isMissing);
        })
        .Where(r => r.IsMissing)
        .Select(r =>
          r.Version == null ? $"{r.Username}/{r.Name}" : $"{r.Username}/{r.Name}:{r.Version}"
        )
        .Distinct()
        .ToArray();

      if (missingRefs.Length > 0)
      {
        var found = lookup.Keys.Select(x => $"{x.Username}/{x.Name}:{x.Version}").ToArray();
        return new MultipleEntityNotFound(
          "Resolvers not found",
          typeof(ResolverPrincipal),
          missingRefs,
          found
        ).ToException();
      }

      // Map through input refs in order to preserve order and duplicates
      var resolvers = resolverRefs
        .Select(r =>
        {
          var version = r.Version ?? latestVersionLookup[(r.Username, r.Name)];
          var entity = lookup[(r.Username, r.Name, version)];
          return new ResolverVersionWithIdentity(
            entity.Resolver.User.Username,
            entity.Resolver.Name,
            entity.ToPrincipal()
          );
        })
        .ToArray();

      logger.LogInformation(
        "Resolver References: {@ResolverReferences}",
        resolvers.Select(x => x.Principal.Id)
      );

      return resolvers;
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed searching resolver versions '{@References}'",
        resolverRefs.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<ResolverVersion?>> GetVersion(
    string username,
    string name,
    ulong version
  )
  {
    try
    {
      logger.LogInformation(
        "Getting resolver version '{Username}/{Name}:{Version}'",
        username,
        name,
        version
      );
      var resolver = await db
        .ResolverVersions.Include(x => x.Resolver)
        .ThenInclude(x => x.User)
        .Where(x =>
          x.Resolver.User.Username == username && x.Resolver.Name == name && x.Version == version
        )
        .FirstOrDefaultAsync();

      return resolver?.ToDomain();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to get resolver version '{Username}/{Name}:{Version}'",
        username,
        name,
        version
      );
      return e;
    }
  }

  public async Task<Result<ResolverVersion?>> GetVersion(string username, string name)
  {
    try
    {
      logger.LogInformation("Getting resolver version '{Username}/{Name}'", username, name);
      var resolver = await db
        .ResolverVersions.Include(x => x.Resolver)
        .ThenInclude(x => x.User)
        .Where(x => x.Resolver.User.Username == username && x.Resolver.Name == name)
        .OrderByDescending(x => x.Version)
        .FirstOrDefaultAsync();

      return resolver?.ToDomain();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to get resolver version '{Username}/{Name}'", username, name);
      return e;
    }
  }

  public async Task<Result<ResolverVersion?>> GetVersion(string userId, Guid id, ulong version)
  {
    try
    {
      logger.LogInformation(
        "Getting resolver version for User '{UserId}', Resolver: '{ResolverId}', Version: {Version}'",
        userId,
        id,
        version
      );
      var resolver = await db
        .ResolverVersions.Include(x => x.Resolver)
        .Where(x => x.Resolver.UserId == userId && x.Resolver.Id == id && x.Version == version)
        .FirstOrDefaultAsync();

      return resolver?.ToDomain();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to get resolver version: User '{UserId}', Resolver '{Name}', Version {Version}'",
        userId,
        id,
        version
      );
      return e;
    }
  }

  public async Task<Result<ResolverVersion?>> GetVersionById(Guid versionId)
  {
    try
    {
      logger.LogInformation("Getting resolver version with Id '{VersionId}'", versionId);
      var version = await db
        .ResolverVersions.Where(x => x.Id == versionId)
        .Include(x => x.Resolver)
        .FirstOrDefaultAsync();

      return version?.ToDomain();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed getting resolver version with Id '{VersionId}'", versionId);
      return e;
    }
  }

  public async Task<Result<ResolverVersionPrincipal?>> CreateVersion(
    string username,
    string name,
    ResolverVersionRecord record,
    ResolverVersionProperty property
  )
  {
    try
    {
      logger.LogInformation(
        "Creating resolver version for '{Username}/{Name}' with Record {@Record} and Property {@Property} ",
        username,
        name,
        record.ToJson(),
        property.ToJson()
      );

      var resolver = await db
        .Resolvers.Include(x => x.User)
        .Where(x => x.User.Username == username && x.Name == name)
        .FirstOrDefaultAsync();

      if (resolver == null)
        return (ResolverVersionPrincipal?)null;

      var latest =
        db.ResolverVersions.Where(x => x.ResolverId == resolver.Id).Max(x => x.Version as ulong?)
        ?? 0;

      var data = new ResolverVersionData();
      data = data.HydrateData(record).HydrateData(property) with
      {
        ResolverId = resolver.Id,
        Resolver = null!,
        Version = latest + 1,
        CreatedAt = DateTime.UtcNow,
      };

      var r = db.ResolverVersions.Add(data);
      await db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to create resolver version for '{Username}/{Name}' with Record {@Record} and Property {@Property} ",
        username,
        name,
        record.ToJson(),
        property.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<ResolverVersionPrincipal?>> CreateVersion(
    string userId,
    Guid id,
    ResolverVersionRecord record,
    ResolverVersionProperty property
  )
  {
    try
    {
      logger.LogInformation(
        "Creating resolver version for User '{UserId}', Resolver '{Id}' with Record {@Record} and Property {@Property} ",
        userId,
        id,
        record.ToJson(),
        property.ToJson()
      );

      var resolver = await db
        .Resolvers.Where(x => x.UserId == userId && x.Id == id)
        .FirstOrDefaultAsync();

      if (resolver == null)
        return (ResolverVersionPrincipal?)null;

      var latest =
        db.ResolverVersions.Where(x => x.ResolverId == resolver.Id).Max(x => x.Version as ulong?)
        ?? 0;

      var data = new ResolverVersionData();
      data = data.HydrateData(record).HydrateData(property) with
      {
        ResolverId = resolver.Id,
        Resolver = null!,
        Version = latest + 1,
        CreatedAt = DateTime.UtcNow,
      };

      var r = db.ResolverVersions.Add(data);
      await db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to create resolver version for User '{UserId}', Resolver '{Id}' with Record {@Record} and Property {@Property}",
        userId,
        id,
        record.ToJson(),
        property.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<ResolverVersionPrincipal?>> UpdateVersion(
    string username,
    string name,
    ulong version,
    ResolverVersionRecord v2
  )
  {
    try
    {
      logger.LogInformation(
        "Updating resolver '{Username}/{Name}:{Version}' with Record {@Record}",
        username,
        name,
        version,
        v2.ToJson()
      );

      var v1 = await db
        .ResolverVersions.Include(x => x.Resolver)
        .ThenInclude(x => x.User)
        .Where(x =>
          x.Version == version && x.Resolver.Name == name && x.Resolver.User.Username == username
        )
        .FirstOrDefaultAsync();

      if (v1 == null)
        return (ResolverVersionPrincipal?)null;

      var v3 = v1.HydrateData(v2) with { Resolver = null! };

      var r = db.ResolverVersions.Update(v3);
      await db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to update resolver '{Username}/{Name}:{Version}' with Record {@Record}",
        username,
        name,
        version,
        v2.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<ResolverVersionPrincipal?>> UpdateVersion(
    string userId,
    Guid id,
    ulong version,
    ResolverVersionRecord v2
  )
  {
    try
    {
      logger.LogInformation(
        "Updating resolver for User '{UserId}', Resolver '{Id}' with Record {@Record}",
        userId,
        id,
        v2.ToJson()
      );

      var v1 = await db
        .ResolverVersions.Include(x => x.Resolver)
        .Where(x => x.Version == version && x.Resolver.Id == id && x.Resolver.UserId == userId)
        .FirstOrDefaultAsync();

      if (v1 == null)
        return (ResolverVersionPrincipal?)null;

      var v3 = v1.HydrateData(v2) with { Resolver = null! };

      var r = db.ResolverVersions.Update(v3);
      await db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to update resolver for User '{UserId}', Resolver '{Id}' with Record {@Record}",
        userId,
        id,
        v2.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<ResolverVersionPrincipal?>> UpdateAndCreateVersion(
    string username,
    string name,
    ResolverMetadata metadata,
    ResolverVersionRecord record,
    ResolverVersionProperty property
  )
  {
    await using var transaction = await db.Database.BeginTransactionAsync();
    try
    {
      logger.LogInformation(
        "Atomically updating resolver and creating version for '{Username}/{Name}'",
        username,
        name
      );

      var resolver = await db
        .Resolvers.Include(x => x.User)
        .Where(x => x.User.Username == username && x.Name == name)
        .FirstOrDefaultAsync();

      if (resolver == null)
      {
        await transaction.CommitAsync();
        return (ResolverVersionPrincipal?)null;
      }

      var updated = resolver.HydrateData(metadata) with { User = null! };
      db.Resolvers.Update(updated);
      await db.SaveChangesAsync();

      var latest =
        db.ResolverVersions.Where(x => x.ResolverId == updated.Id).Max(x => x.Version as ulong?)
        ?? 0;

      var data = new ResolverVersionData();
      data = data.HydrateData(record).HydrateData(property) with
      {
        ResolverId = updated.Id,
        Resolver = null!,
        Version = latest + 1,
        CreatedAt = DateTime.UtcNow,
      };

      var r = db.ResolverVersions.Add(data);
      await db.SaveChangesAsync();

      await transaction.CommitAsync();
      return r.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      await transaction.RollbackAsync();
      logger.LogError(
        e,
        "Failed to atomically update resolver and create version for '{Username}/{Name}'",
        username,
        name
      );
      return e;
    }
  }
}
