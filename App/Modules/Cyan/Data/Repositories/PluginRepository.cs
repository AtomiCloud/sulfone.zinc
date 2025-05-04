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

public class PluginRepository(MainDbContext db, ILogger<PluginRepository> logger)
  : IPluginRepository
{
  public async Task<Result<IEnumerable<PluginPrincipal>>> Search(PluginSearch search)
  {
    try
    {
      logger.LogInformation(
        "Searching for plugins with Search Params '{@SearchParams}'",
        search.ToJson()
      );
      var plugins = db.Plugins.AsQueryable();

      if (search.Owner != null)
        plugins = plugins.Include(x => x.User).Where(x => x.User.Username == search.Owner);

      if (search.Search != null)
        plugins = plugins
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

      plugins = plugins.Skip(search.Skip).Take(search.Limit);
      var a = await plugins.ToArrayAsync();
      return a.Select(x => x.ToPrincipal()).ToResult();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed getting plugins with Search Params '{@SearchParams}'",
        search.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<Plugin?>> Get(string userId, Guid id)
  {
    try
    {
      logger.LogInformation("Getting plugin with '{ID}'", id);
      var plugin = await db
        .Plugins.Include(x => x.User)
        .Where(x => x.Id == id && x.UserId == userId)
        .Include(x => x.Likes)
        .Include(x => x.Versions)
        .ThenInclude(x => x.Templates)
        .FirstOrDefaultAsync();

      if (plugin == null)
        return (Plugin?)null;

      var info = new PluginInfo
      {
        Downloads = plugin.Downloads,
        Dependencies = (uint)plugin.Versions.Sum(x => x.Templates.Count()),
        Stars = (uint)plugin.Likes.Count(),
      };
      return plugin?.ToDomain(info);
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed getting plugin '{PluginId}'", id);
      return e;
    }
  }

  public async Task<Result<Plugin?>> Get(string username, string name)
  {
    try
    {
      logger.LogInformation("Getting plugin '{Username}/{Name}'", username, name);
      var plugin = await db
        .Plugins.Include(x => x.User)
        .Where(x => x.User.Username == username && x.Name == name)
        .Include(x => x.Likes)
        .Include(x => x.User)
        .Include(x => x.Versions)
        .ThenInclude(x => x.Templates)
        .FirstOrDefaultAsync();

      if (plugin == null)
        return (Plugin?)null;

      var info = new PluginInfo
      {
        Downloads = plugin.Downloads,
        Dependencies = (uint)plugin.Versions.Sum(x => x.Templates.Count()),
        Stars = (uint)plugin.Likes.Count(),
      };
      return plugin?.ToDomain(info);
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed getting plugin '{Username}/{Name}'", username, name);
      return e;
    }
  }

  public async Task<Result<PluginPrincipal>> Create(
    string userId,
    PluginRecord record,
    PluginMetadata metadata
  )
  {
    try
    {
      var data = new PluginData();
      data = data.HydrateData(record).HydrateData(metadata) with
      {
        Downloads = 0,
        User = null!,
        UserId = userId,
      };
      logger.LogInformation(
        "Creating plugin {UserId} with Record {@Record} and Metadata {@Metadata}",
        userId,
        record.ToJson(),
        metadata.ToJson()
      );

      var r = db.Plugins.Add(data);
      await db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (UniqueConstraintException e)
    {
      logger.LogError(
        e,
        "Failed to create plugin due to conflict: {UserId} with Record {@Record} and Metadata {@Metadata}",
        userId,
        record.ToJson(),
        metadata.ToJson()
      );
      return new AlreadyExistException(
        "Failed to create Plugin due to conflicting with existing record",
        e,
        typeof(PluginPrincipal)
      );
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed updating plugin {UserId} with Record {@Record} and Metadata {@Metadata}",
        userId,
        record.ToJson(),
        metadata.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<PluginPrincipal?>> Update(string userId, Guid id, PluginMetadata v2)
  {
    try
    {
      var v1 = await db.Plugins.Where(x => x.UserId == userId && x.Id == id).FirstOrDefaultAsync();
      if (v1 == null)
        return (PluginPrincipal?)null;

      var v3 = v1.HydrateData(v2) with { User = null! };
      var updated = db.Plugins.Update(v3);
      await db.SaveChangesAsync();
      return updated.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to update Plugin with User ID '{UserID}' and Plugin ID '{PluginID}': {@Record}",
        userId,
        id,
        v2.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<PluginPrincipal?>> Update(
    string username,
    string name,
    PluginMetadata v2
  )
  {
    try
    {
      var v1 = await db
        .Plugins.Include(x => x.User)
        .Where(x => x.User.Username == username && x.Name == name)
        .FirstOrDefaultAsync();
      if (v1 == null)
        return (PluginPrincipal?)null;

      var v3 = v1.HydrateData(v2) with { User = null! };
      var updated = db.Plugins.Update(v3);
      await db.SaveChangesAsync();
      return updated.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to update Plugin '{Username}/{Name}': {@Record}",
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
      var a = await db.Plugins.Where(x => x.UserId == userId && x.Id == id).FirstOrDefaultAsync();
      if (a == null)
        return (Unit?)null;

      db.Plugins.Remove(a);
      await db.SaveChangesAsync();
      return new Unit();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to delete Plugin '{PluginId}' for User '{UserId}", id, userId);
      return e;
    }
  }

  public async Task<Result<Unit?>> Like(string likerId, string username, string name, bool like)
  {
    try
    {
      // check if like already exist
      var likeExist = await db
        .PluginLikes.Include(x => x.Plugin)
        .ThenInclude(x => x.User)
        .AnyAsync(x =>
          x.UserId == likerId && x.Plugin.User.Username == username && x.Plugin.Name == name
        );

      if (like == likeExist)
        return new LikeConflictError(
          "Failed to like plugins",
          $"{username}/{name}",
          "plugin",
          like ? "like" : "unlike"
        ).ToException();

      var p = await db
        .Plugins.Include(x => x.User)
        .FirstOrDefaultAsync(x => x.User.Username == username && x.Name == name);

      if (p == null)
        return (Unit?)null;

      if (like)
      {
        // if like, check for conflict
        var l = new PluginLikeData
        {
          UserId = likerId,
          User = null!,
          PluginId = p.Id,
          Plugin = null!,
        };
        var r = db.PluginLikes.Add(l);
        await db.SaveChangesAsync();
        return new Unit();
      }
      else
      {
        var l = await db.PluginLikes.FirstOrDefaultAsync(x =>
          x.UserId == likerId && x.PluginId == p.Id
        );
        if (l == null)
        {
          logger.LogError(
            "Race Condition, Failed to unlike Plugin '{Username}/{Name}' for User '{UserId}'. User-Plugin-Like entry does not exist even though it exist at the start of the query",
            username,
            name,
            likerId
          );

          return new LikeRaceConditionError(
            "Failed to like plugins",
            $"{username}/{name}",
            "plugin",
            like ? "like" : "unlike"
          ).ToException();
        }

        db.Remove(l with { User = null!, Plugin = null! });
        await db.SaveChangesAsync();
        return new Unit();
      }
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to {Like} Plugin '{Username}/{Name}' for User '{UserId}",
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
      var plugin = await db
        .Plugins.Include(x => x.User)
        .Where(x => x.User.Username == username && x.Name == name)
        .FirstOrDefaultAsync();
      if (plugin == null)
        return (uint?)null;

      plugin = plugin with { Downloads = plugin.Downloads + 1, User = null! };

      var updated = db.Plugins.Update(plugin);
      await db.SaveChangesAsync();
      return updated.Entity.Downloads;
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to increment download count for Plugin '{Username}/{Name}'",
        username,
        name
      );
      return e;
    }
  }

  public async Task<Result<IEnumerable<PluginVersionPrincipal>>> SearchVersion(
    string username,
    string name,
    PluginVersionSearch version
  )
  {
    try
    {
      var query = db
        .PluginVersions.Include(x => x.Plugin)
        .ThenInclude(x => x.User)
        .Where(x => x.Plugin.User.Username == username && x.Plugin.Name == name)
        .AsQueryable();

      if (version.Search != null)
        query = query.Where(x =>
          EF.Functions.ILike(x.Description, $"%{version.Search}%")
          || version.Search.Contains(version.Search)
        );

      var plugins = await query.Skip(version.Skip).Take(version.Limit).ToArrayAsync();

      return plugins.Select(x => x.ToPrincipal()).ToResult();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed searching plugin version of Plugin '{Username}/{Name}' with {@Params}",
        username,
        name,
        version.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<IEnumerable<PluginVersionPrincipal>>> SearchVersion(
    string userId,
    Guid id,
    PluginVersionSearch version
  )
  {
    try
    {
      var query = db
        .PluginVersions.Include(x => x.Plugin)
        .Where(x => x.Plugin.UserId == userId && x.Plugin.Id == id)
        .AsQueryable();

      if (version.Search != null)
        query = query.Where(x =>
          EF.Functions.ILike(x.Description, $"%{version.Search}%")
          || version.Search.Contains(version.Search)
        );

      var plugins = await query.Skip(version.Skip).Take(version.Limit).ToArrayAsync();

      return plugins.Select(x => x.ToPrincipal()).ToResult();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed searching plugin version of Plugin '{PluginId}' of User '{UserId}' with {@Params}",
        id,
        userId,
        version.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<IEnumerable<PluginVersionPrincipal>>> GetAllVersion(
    IEnumerable<PluginVersionRef> references
  )
  {
    var pluginRefs = references as PluginVersionRef[] ?? references.ToArray();
    try
    {
      logger.LogInformation("Getting all plugin versions {@References}", pluginRefs.ToJson());
      if (pluginRefs.IsNullOrEmpty())
        return Array.Empty<PluginVersionPrincipal>();
      var query = db.PluginVersions.Include(x => x.Plugin).ThenInclude(x => x.User).AsQueryable();

      var predicate = PredicateBuilder.New<PluginVersionData>(true);

      predicate = pluginRefs.Aggregate(
        predicate,
        (c, r) =>
          r.Version != null
            ? c.Or(x =>
              x.Plugin.Name == r.Name
              && x.Plugin.User.Username == r.Username
              && x.Version == r.Version
            )
            : c.Or(x => x.Plugin.Name == r.Name && x.Plugin.User.Username == r.Username)
      );

      var all = await query.Where(predicate).ToArrayAsync();
      var grouped = all.GroupBy(x => new { x.Plugin.Name, x.Plugin.User.Username })
        .Select(g => g.OrderByDescending(o => o.Version).First())
        .ToArray();

      var plugins = grouped.Select(x => x.ToPrincipal()).ToArray();

      logger.LogInformation("Plugin References: {@PluginReferences}", plugins.Select(x => x.Id));

      if (plugins.Length != pluginRefs.Length)
      {
        var found = grouped
          .Select(x => $"{x.Plugin.User.Username}/{x.Plugin.Name}:{x.Version}")
          .ToArray();
        var search = pluginRefs.Select(x => $"{x.Username}/{x.Name}:{x.Version}");
        var notFound = search.Except(found);
        return new MultipleEntityNotFound(
          "Plugins not found",
          typeof(PluginPrincipal),
          notFound.ToArray(),
          found.ToArray()
        ).ToException();
      }

      return plugins;
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed searching plugin versions '{@References}'", pluginRefs.ToJson());
      return e;
    }
  }

  public async Task<Result<PluginVersion?>> GetVersion(string username, string name, ulong version)
  {
    try
    {
      logger.LogInformation(
        "Getting plugin version '{Username}/{Name}:{Version}'",
        username,
        name,
        version
      );
      var plugin = await db
        .PluginVersions.Include(x => x.Plugin)
        .ThenInclude(x => x.User)
        .Where(x =>
          x.Plugin.User.Username == username && x.Plugin.Name == name && x.Version == version
        )
        .FirstOrDefaultAsync();

      return plugin?.ToDomain();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to get plugin version '{Username}/{Name}:{Version}'",
        username,
        name,
        version
      );
      return e;
    }
  }

  public async Task<Result<PluginVersion?>> GetVersion(string username, string name)
  {
    try
    {
      logger.LogInformation("Getting plugin version '{Username}/{Name}'", username, name);
      var plugin = await db
        .PluginVersions.Include(x => x.Plugin)
        .ThenInclude(x => x.User)
        .Where(x => x.Plugin.User.Username == username && x.Plugin.Name == name)
        .OrderByDescending(x => x.Version)
        .FirstOrDefaultAsync();

      return plugin?.ToDomain();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to get plugin version '{Username}/{Name}'", username, name);
      return e;
    }
  }

  public async Task<Result<PluginVersion?>> GetVersion(string userId, Guid id, ulong version)
  {
    try
    {
      logger.LogInformation(
        "Getting plugin version for User '{UserId}', Plugin: '{PluginId}', Version: {Version}'",
        userId,
        id,
        version
      );

      var plugin = await db
        .PluginVersions.Include(x => x.Plugin)
        .Where(x => x.Plugin.UserId == userId && x.Plugin.Id == id && x.Version == version)
        .FirstOrDefaultAsync();

      return plugin?.ToDomain();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to get plugin version: User '{UserId}', Plugin '{Name}', Version {Version}'",
        userId,
        id,
        version
      );
      return e;
    }
  }

  public async Task<Result<PluginVersionPrincipal?>> CreateVersion(
    string username,
    string name,
    PluginVersionRecord record,
    PluginVersionProperty property
  )
  {
    try
    {
      logger.LogInformation(
        "Creating plugin version for '{Username}/{Name}' with Record {@Record} and Property {@Property} ",
        username,
        name,
        record.ToJson(),
        property.ToJson()
      );

      var plugin = await db
        .Plugins.Include(x => x.User)
        .Where(x => x.User.Username == username && x.Name == name)
        .FirstOrDefaultAsync();

      if (plugin == null)
        return (PluginVersionPrincipal?)null;

      logger.LogInformation("Getting latest version for '{Username}/{Name}'", username, name);
      var latest =
        db.PluginVersions.Where(x => x.PluginId == plugin.Id).Max(x => x.Version as ulong?) ?? 0;

      logger.LogInformation(
        "Latest version for '{Username}/{Name}' is {Version}",
        username,
        name,
        latest
      );

      var data = new PluginVersionData();
      data = data.HydrateData(record).HydrateData(property) with
      {
        PluginId = plugin.Id,
        Plugin = null!,
        Version = latest + 1,
        CreatedAt = DateTime.UtcNow,
      };

      var r = db.PluginVersions.Add(data);
      await db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to create plugin version for '{Username}/{Name}' with Record {@Record} and Property {@Property} ",
        username,
        name,
        record.ToJson(),
        property.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<PluginVersionPrincipal?>> CreateVersion(
    string userId,
    Guid id,
    PluginVersionRecord record,
    PluginVersionProperty property
  )
  {
    try
    {
      logger.LogInformation(
        "Creating plugin version for User '{UserId}', Plugin '{Id}' with Record {@Record} and Property {@Property} ",
        userId,
        id,
        record.ToJson(),
        property.ToJson()
      );

      var plugin = await db
        .Plugins.Where(x => x.UserId == userId && x.Id == id)
        .FirstOrDefaultAsync();

      if (plugin == null)
        return (PluginVersionPrincipal?)null;

      var latest = db.PluginVersions.Where(x => x.PluginId == plugin.Id).Max(x => x.Version);

      var data = new PluginVersionData();
      data = data.HydrateData(record).HydrateData(property) with
      {
        PluginId = plugin.Id,
        Plugin = null!,
        Version = latest + 1,
        CreatedAt = DateTime.Now,
      };

      var r = db.PluginVersions.Add(data);
      await db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to create plugin version for User '{UserId}', Plugin '{Id}' with Record {@Record} and Property {@Property}",
        userId,
        id,
        record.ToJson(),
        property.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<PluginVersionPrincipal?>> UpdateVersion(
    string username,
    string name,
    ulong version,
    PluginVersionRecord v2
  )
  {
    try
    {
      logger.LogInformation(
        "Updating plugin '{Username}/{Name}:{Version}' with Record {@Record}",
        username,
        name,
        version,
        v2.ToJson()
      );

      var v1 = await db
        .PluginVersions.Include(x => x.Plugin)
        .ThenInclude(x => x.User)
        .Where(x =>
          x.Version == version && x.Plugin.Name == name && x.Plugin.User.Username == username
        )
        .FirstOrDefaultAsync();

      if (v1 == null)
        return (PluginVersionPrincipal?)null;

      var v3 = v1.HydrateData(v2) with { Plugin = null! };

      var r = db.PluginVersions.Update(v3);
      await db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to update plugin '{Username}/{Name}:{Version}' with Record {@Record}",
        username,
        name,
        version,
        v2.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<PluginVersionPrincipal?>> UpdateVersion(
    string userId,
    Guid id,
    ulong version,
    PluginVersionRecord v2
  )
  {
    try
    {
      logger.LogInformation(
        "Updating plugin for User '{UserId}', Plugin '{Id}' with Record {@Record}",
        userId,
        id,
        v2.ToJson()
      );

      var v1 = await db
        .PluginVersions.Include(x => x.Plugin)
        .Where(x => x.Version == version && x.Plugin.Id == id && x.Plugin.UserId == userId)
        .FirstOrDefaultAsync();

      if (v1 == null)
        return (PluginVersionPrincipal?)null;

      var v3 = v1.HydrateData(v2) with { Plugin = null! };

      var r = db.PluginVersions.Update(v3);
      await db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to update plugin for User '{UserId}', Plugin '{Id}' with Record {@Record}",
        userId,
        id,
        v2.ToJson()
      );
      return e;
    }
  }
}
