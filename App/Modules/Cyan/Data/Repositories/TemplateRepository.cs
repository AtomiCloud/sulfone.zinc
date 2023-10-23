using System.Runtime.CompilerServices;
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
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace App.Modules.Cyan.Data.Repositories;

public class TemplateRepository : ITemplateRepository
{
  private readonly MainDbContext _db;
  private readonly ILogger<TemplateRepository> _logger;

  public TemplateRepository(MainDbContext db, ILogger<TemplateRepository> logger)
  {
    this._db = db;
    this._logger = logger;
  }

  public async Task<Result<IEnumerable<TemplatePrincipal>>> Search(TemplateSearch search)
  {
    try
    {
      this._logger.LogInformation("Searching for templates with Search Params '{@SearchParams}'", search.ToJson());
      var templates = this._db.Templates.AsQueryable();

      if (search.Owner != null)
        templates = templates
          .Include(x => x.User)
          .Where(x => x.User.Username == search.Owner);

      if (search.Search != null)
        templates = templates
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

      templates = templates.Skip(search.Skip).Take(search.Limit);
      var a = await templates
        .ToArrayAsync();
      return a.Select(x => x.ToPrincipal()).ToResult();
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e, "Failed getting templates with Search Params '{@SearchParams}'", search.ToJson());
      return e;
    }
  }

  public async Task<Result<Template?>> Get(string userId, Guid id)
  {
    try
    {
      this._logger.LogInformation("Getting template with '{ID}'", id);
      var template = await this._db.Templates
        .Where(x => x.Id == id && x.UserId == userId)
        .Include(x => x.Likes)
        .Include(x => x.User)
        .Include(x => x.Versions)
        .FirstOrDefaultAsync();

      if (template == null) return (Template?)null;

      var info = new TemplateInfo { Downloads = template.Downloads, Stars = (uint)template.Likes.Count() };
      return template?.ToDomain(info);
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e, "Failed getting template '{TemplateId}'", id);
      return e;
    }
  }

  public async Task<Result<Template?>> Get(string username, string name)
  {
    try
    {
      this._logger.LogInformation("Getting template '{Username}/{Name}'", username, name);
      var template = await this._db.Templates
        .Include(x => x.Likes)
        .Include(x => x.Versions)
        .Include(x => x.User)
        .Where(x => x.User.Username == username && x.Name == name)
        .FirstOrDefaultAsync();

      if (template == null) return (Template?)null;

      var info = new TemplateInfo { Downloads = template.Downloads, Stars = (uint)template.Likes.Count() };
      return template?.ToDomain(info);
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e, "Failed getting template '{Username}/{Name}'", username, name);
      return e;
    }
  }

  public async Task<Result<TemplatePrincipal>> Create(string userId, TemplateRecord record, TemplateMetadata metadata)
  {
    try
    {
      this._logger.LogInformation("Creating template {UserId} with Record {@Record} and Metadata {@Metadata}", userId,
        record.ToJson(), metadata.ToJson());
      var data = new TemplateData();
      data = data
          .HydrateData(record)
          .HydrateData(metadata) with
      {
        Downloads = 0,
        User = null!,
        UserId = userId,
      };
      this._logger.LogInformation("Creating template with Template Data: {@Template}", data.ToJson());
      var r = this._db.Templates.Add(data);
      await this._db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (UniqueConstraintException e)
    {
      this._logger.LogError(e,
        "Failed to create template due to conflict: {UserId} with Record {@Record} and Metadata {@Metadata}", userId,
        record.ToJson(), metadata.ToJson());
      return new AlreadyExistException("Failed to create Template due to conflicting with existing record", e,
        typeof(TemplatePrincipal));
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e, "Failed updating template {UserId} with Record {@Record} and Metadata {@Metadata}", userId,
          record.ToJson(), metadata.ToJson());
      return e;
    }
  }

  public async Task<Result<TemplatePrincipal?>> Update(string userId, Guid id, TemplateMetadata v2)
  {
    try
    {
      var v1 = await this._db.Templates
        .Where(x => x.UserId == userId && x.Id == id)
        .FirstOrDefaultAsync();
      if (v1 == null) return (TemplatePrincipal?)null;

      var v3 = v1.HydrateData(v2) with { User = null!, };
      var updated = this._db.Templates.Update(v3);
      await this._db.SaveChangesAsync();
      return updated.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      this._logger.LogError(e,
        "Failed to update Template with User ID '{UserID}' and Template ID '{TemplateID}': {@Record}",
        userId, id, v2.ToJson());
      return e;
    }
  }

  public async Task<Result<TemplatePrincipal?>> Update(string username, string name, TemplateMetadata v2)
  {
    try
    {
      var v1 = await this._db.Templates
        .Include(x => x.User)
        .Where(x => x.User.Username == username && x.Name == name)
        .FirstOrDefaultAsync();
      if (v1 == null) return (TemplatePrincipal?)null;

      var v3 = v1.HydrateData(v2) with { User = null!, };
      var updated = this._db.Templates.Update(v3);
      await this._db.SaveChangesAsync();
      return updated.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      this._logger.LogError(e, "Failed to update Template '{Username}/{Name}': {@Record}",
        username, name, v2.ToJson());
      return e;
    }
  }

  public async Task<Result<Unit?>> Delete(string userId, Guid id)
  {
    try
    {
      var a = await this._db.Templates
        .Where(x => x.UserId == userId && x.Id == id)
        .FirstOrDefaultAsync();
      if (a == null) return (Unit?)null;

      this._db.Templates.Remove(a);
      await this._db.SaveChangesAsync();
      return new Unit();
    }
    catch (Exception e)
    {
      this._logger.LogError(e, "Failed to delete Template '{TemplateId}' for User '{UserId}", id, userId);
      return e;
    }
  }

  public async Task<Result<Unit?>> Like(string likerId, string username, string name, bool like)
  {
    try
    {
      // check if like already exist
      var likeExist = await this._db.TemplateLikes
        .Include(x => x.Template)
        .ThenInclude(x => x.User)
        .AnyAsync(x => x.UserId == likerId
                       && x.Template.User.Username == username
                       && x.Template.Name == name
        );

      if (like == likeExist)
        return new LikeConflictError("Failed to like templates", $"{username}/{name}", "template",
          like ? "like" : "unlike").ToException();

      var p = await this._db.Templates
        .Include(x => x.User)
        .FirstOrDefaultAsync(x => x.User.Username == username && x.Name == name);

      if (p == null) return (Unit?)null;

      if (like)
      {
        // if like, check for conflict
        var l = new TemplateLikeData { UserId = likerId, User = null!, TemplateId = p.Id, Template = null! };
        var r = this._db.TemplateLikes.Add(l);
        await this._db.SaveChangesAsync();
        return new Unit();
      }
      else
      {
        var l = await this._db.TemplateLikes
          .FirstOrDefaultAsync(x => x.UserId == likerId && x.TemplateId == p.Id);
        if (l == null)
        {
          this._logger.LogError(
            "Race Condition, Failed to unlike Template '{Username}/{Name}' for User '{UserId}'. User-Template-Like entry does not exist even though it exist at the start of the query",
            username, name, likerId);

          return new LikeRaceConditionError("Failed to like templates", $"{username}/{name}", "template",
            like ? "like" : "unlike").ToException();
        }

        this._logger.LogInformation("Removing {@Like}", l);
        this._db.TemplateLikes.Remove(l with { Template = null!, User = null!, });
        await this._db.SaveChangesAsync();
        return new Unit();
      }
    }
    catch (Exception e)
    {
      this._logger.LogError(e, "Failed to {Like} Template '{Username}/{Name}' for User '{UserId}",
        like ? "like" : "unlike", username, name, likerId);
      return e;
    }
  }

  public async Task<Result<uint?>> IncrementDownload(string username, string name)
  {
    try
    {
      var template = await this._db.Templates
        .Include(x => x.User)
        .Where(x => x.User.Username == username && x.Name == name)
        .FirstOrDefaultAsync();
      if (template == null) return (uint?)null;

      template = template with { Downloads = template.Downloads + 1, User = null!, };

      var updated = this._db.Templates.Update(template);
      await this._db.SaveChangesAsync();
      return updated.Entity.Downloads;
    }
    catch (Exception e)
    {
      this._logger.LogError(e, "Failed to increment download count for Template '{Username}/{Name}'",
        username, name);
      return e;
    }
  }

  public async Task<Result<IEnumerable<TemplateVersionPrincipal>>> SearchVersion(string username, string name,
    TemplateVersionSearch version)
  {
    try
    {
      var query = this._db.TemplateVersions
        .Include(x => x.Template)
        .ThenInclude(x => x.User)
        .Where(x => x.Template.User.Username == username && x.Template.Name == name)
        .AsQueryable();

      if (version.Search != null)
        query = query.Where(x =>
          EF.Functions.ILike(x.Description, $"%{version.Search}%") ||
          version.Search.Contains(version.Search)
        );

      var templates = await query
        .Skip(version.Skip)
        .Take(version.Limit)
        .ToArrayAsync();

      return templates.Select(x => x.ToPrincipal()).ToResult();
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e, "Failed searching template version of Template '{Username}/{Name}' with {@Params}",
          username, name, version.ToJson());
      return e;
    }
  }

  public async Task<Result<IEnumerable<TemplateVersionPrincipal>>> SearchVersion(string userId, Guid id,
    TemplateVersionSearch version)
  {
    try
    {
      var query = this._db.TemplateVersions
        .Include(x => x.Template)
        .Where(x => x.Template.UserId == userId && x.Template.Id == id)
        .AsQueryable();

      if (version.Search != null)
        query = query.Where(x =>
          EF.Functions.ILike(x.Description, $"%{version.Search}%") ||
          version.Search.Contains(version.Search)
        );

      var templates = await query
        .Skip(version.Skip)
        .Take(version.Limit)
        .ToArrayAsync();

      return templates.Select(x => x.ToPrincipal()).ToResult();
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e, "Failed searching template version of Template '{TemplateId}' of User '{UserId}' with {@Params}",
          id, userId, version.ToJson());
      return e;
    }
  }

  public async Task<Result<TemplateVersion?>> GetVersion(string username, string name, ulong version)
  {
    try
    {
      this._logger.LogInformation("Getting template version '{Username}/{Name}:{Version}'", username, name, version);
      var template = await this._db.TemplateVersions
        .Include(x => x.Template)
        .ThenInclude(x => x.User)
        .Include(x => x.Plugins)
        .ThenInclude(x => x.Plugin)
        .Include(x => x.Processors)
        .ThenInclude(x => x.Processor)
        .Where(x => x.Template.User.Username == username && x.Template.Name == name && x.Version == version)
        .FirstOrDefaultAsync();
      return template?.ToDomain();
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e, "Failed to get template version '{Username}/{Name}:{Version}'", username, name, version);
      return e;
    }
  }

  public async Task<Result<TemplateVersion?>> GetVersion(string userId, Guid id, ulong version)
  {
    try
    {
      this._logger.LogInformation(
        "Getting template version for User '{UserId}', Template: '{TemplateId}', Version: {Version}'", userId, id,
        version);
      var template = await this._db.TemplateVersions
        .Include(x => x.Template)
        .Include(x => x.Plugins)
        .Include(x => x.Processors)
        .Where(x => x.Template.UserId == userId && x.Template.Id == id && x.Version == version)
        .FirstOrDefaultAsync();

      return template?.ToDomain();
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e, "Failed to get template version: User '{UserId}', Template '{Name}', Version {Version}'", userId,
          id,
          version);
      return e;
    }
  }

  public async Task<Result<TemplateVersionPrincipal?>> CreateVersion(string username, string name,
    TemplateVersionRecord record,
    TemplateVersionProperty property, IEnumerable<Guid> processors, IEnumerable<Guid> plugins)
  {
    await using var transaction = await this._db.Database.BeginTransactionAsync();
    try
    {
      this._logger.LogInformation(
        "Creating template version for '{Username}/{Name}' with Record {@Record} and Property {@Property} ",
        username, name, record.ToJson(), property.ToJson());

      var template = await this._db.Templates
        .Include(x => x.User)
        .Where(x => x.User.Username == username && x.Name == name)
        .FirstOrDefaultAsync();

      if (template == null)
      {
        await transaction.CommitAsync();
        return (TemplateVersionPrincipal?)null;
      }

      this._logger.LogInformation("Getting latest version for '{Username}/{Name}'", username, name);
      var latest = this._db.TemplateVersions
        .Where(x => x.TemplateId == template.Id)
        .Max(x => x.Version as ulong?) ?? 0;

      this._logger.LogInformation("Obtained latest version for '{Username}/{Name}': {Version}", username, name, latest);

      var data = new TemplateVersionData();
      data = data
          .HydrateData(record)
          .HydrateData(property)
        with
      {
        TemplateId = template.Id,
        Template = null!,
        Version = latest + 1,
        CreatedAt = DateTime.UtcNow,
        Plugins = null!,
        Processors = null!,
      };

      var r = this._db.TemplateVersions.Add(data);
      await this._db.SaveChangesAsync();
      var t = r.Entity.ToPrincipal();

      // save plugin links
      var pluginLinks = plugins.Select(x => new TemplatePluginVersionData
      {
        PluginId = x,
        Plugin = null!,
        TemplateId = t.Id,
        Template = null!,
      });
      this._logger.LogInformation("Saving plugins links for '{Username}/{Name}:{Version}', Plugins: {@Plugins}",
        username, name, latest, pluginLinks.ToJson());
      this._db.TemplatePluginVersions.AddRange(pluginLinks);
      var processorLinks = processors.Select(x => new TemplateProcessorVersionData
      {
        ProcessorId = x,
        Processor = null!,
        TemplateId = t.Id,
        Template = null!,
      });
      this._logger.LogInformation(
        "Saving processors links for '{Username}/{Name}:{Version}', Processors: {@Processors}", username, name, latest,
        processorLinks.ToJson());
      this._db.TemplateProcessorVersions.AddRange(processorLinks);
      await this._db.SaveChangesAsync();
      await transaction.CommitAsync();
      return t;
    }
    catch (Exception e)
    {
      await transaction.RollbackAsync();
      this._logger
        .LogError(e,
          "Failed to create template version for '{Username}/{Name}' with Record {@Record} and Property {@Property} ",
          username, name, record.ToJson(), property.ToJson());
      return e;
    }
  }

  public async Task<Result<TemplateVersionPrincipal?>> CreateVersion(string userId, Guid id,
    TemplateVersionRecord record,
    TemplateVersionProperty property, IEnumerable<Guid> processors, IEnumerable<Guid> plugins)
  {
    await using var transaction = await this._db.Database.BeginTransactionAsync();
    try
    {
      this._logger.LogInformation(
        "Creating template version for User '{UserId}', Template '{Id}' with Record {@Record} and Property {@Property} ",
        userId, id, record.ToJson(), property.ToJson());

      var template = await this._db.Templates
        .Where(x => x.UserId == userId && x.Id == id)
        .FirstOrDefaultAsync();

      if (template == null) return (TemplateVersionPrincipal?)null;

      var latest = this._db.TemplateVersions
        .Where(x => x.TemplateId == template.Id)
        .Max(x => x.Version as ulong?) ?? 0;

      var data = new TemplateVersionData();
      data = data
          .HydrateData(record)
          .HydrateData(property)
        with
      {
        TemplateId = template.Id,
        Template = null!,
        Version = latest + 1,
        CreatedAt = DateTime.UtcNow,
      };

      var r = this._db.TemplateVersions.Add(data);
      await this._db.SaveChangesAsync();
      var t = r.Entity.ToPrincipal();

      // save plugin links
      var pluginLinks = plugins.Select(x => new TemplatePluginVersionData
      {
        PluginId = x,
        Plugin = null!,
        TemplateId = t.Id,
        Template = null!,
      });
      this._db.TemplatePluginVersions.AddRange(pluginLinks);
      var processorLinks = processors.Select(x => new TemplateProcessorVersionData
      {
        ProcessorId = x,
        Processor = null!,
        TemplateId = t.Id,
        Template = null!,
      });
      this._db.TemplateProcessorVersions.AddRange(processorLinks);
      await this._db.SaveChangesAsync();
      await transaction.CommitAsync();
      return t;
    }
    catch (Exception e)
    {
      await transaction.RollbackAsync();
      this._logger
        .LogError(e,
          "Failed to create template version for User '{UserId}', Template '{Id}' with Record {@Record} and Property {@Property}",
          userId, id, record.ToJson(), property.ToJson());
      return e;
    }
  }

  public async Task<Result<TemplateVersionPrincipal?>> UpdateVersion(string username, string name, ulong version,
    TemplateVersionRecord v2)
  {
    try
    {
      this._logger.LogInformation(
        "Updating template '{Username}/{Name}:{Version}' with Record {@Record}",
        username, name, version, v2.ToJson());


      var v1 = await this._db.TemplateVersions
        .Include(x => x.Template)
        .ThenInclude(x => x.User)
        .Where(x => x.Version == version && x.Template.Name == name && x.Template.User.Username == username)
        .FirstOrDefaultAsync();

      if (v1 == null) return (TemplateVersionPrincipal?)null;

      var v3 = v1.HydrateData(v2)
        with
      {
        Template = null!,
      };

      var r = this._db.TemplateVersions.Update(v3);
      await this._db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e,
          "Failed to update template '{Username}/{Name}:{Version}' with Record {@Record}",
          username, name, version, v2.ToJson());
      return e;
    }
  }

  public async Task<Result<TemplateVersionPrincipal?>> UpdateVersion(string userId, Guid id, ulong version,
    TemplateVersionRecord v2)
  {
    try
    {
      this._logger.LogInformation(
        "Updating template for User '{UserId}', Template '{Id}' with Record {@Record}",
        userId, id, v2.ToJson());


      var v1 = await this._db.TemplateVersions
        .Include(x => x.Template)
        .Where(x => x.Version == version && x.Template.Id == id && x.Template.UserId == userId)
        .FirstOrDefaultAsync();

      if (v1 == null) return (TemplateVersionPrincipal?)null;

      var v3 = v1.HydrateData(v2)
        with
      {
        Template = null!,
      };

      var r = this._db.TemplateVersions.Update(v3);
      await this._db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      this._logger
        .LogError(e,
          "Failed to update template for User '{UserId}', Template '{Id}' with Record {@Record}",
          userId, id, v2.ToJson());
      return e;
    }
  }
}
