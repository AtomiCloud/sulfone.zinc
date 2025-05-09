using App.StartUp.Database;
using App.Utility;
using CSharp_Result;
using Domain.Error;
using Domain.Model;
using Domain.Repository;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.Users.Data;

public class UserRepository(MainDbContext db, ILogger<UserRepository> logger) : IUserRepository
{
  public async Task<Result<IEnumerable<UserPrincipal>>> Search(UserSearch search)
  {
    try
    {
      logger.LogInformation("Searching with '{@Search}'", search);
      var query = db.Users.AsQueryable();
      if (!string.IsNullOrWhiteSpace(search.Username))
        query = query.Where(x => EF.Functions.ILike(x.Username, $"%{search.Username}%"));
      if (!string.IsNullOrWhiteSpace(search.Id))
        query = query.Where(x => EF.Functions.ILike(x.Id.ToString(), $"%{search.Id}%"));
      var result = await query.Skip(search.Skip).Take(search.Limit).ToArrayAsync();
      return result.Select(x => x.ToPrincipal()).ToResult();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed search for Athlete with Search: {@Search}", search);
      return e;
    }
  }

  public async Task<Result<User?>> GetById(string id)
  {
    try
    {
      logger.LogInformation("Retrieving User with Id: {Id}", id);
      var user = await db.Users.Where(x => x.Id == id).Include(x => x.Tokens).FirstOrDefaultAsync();
      return user?.ToDomain();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed retrieving User with Id: {Id}", id);
      return e;
    }
  }

  public async Task<Result<User?>> GetByUsername(string username)
  {
    try
    {
      logger.LogInformation("Retrieving User by Username: {Username}", username);
      var user = await db
        .Users.Where(x => x.Username == username)
        .Include(x => x.Tokens)
        .FirstOrDefaultAsync();
      return user?.ToDomain();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed retrieving User by Username: {Username}", username);
      return e;
    }
  }

  public async Task<Result<bool>> Exists(string username)
  {
    try
    {
      return await db.Users.AnyAsync(x => x.Username == username);
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to check username exist {Username}", username);
      return e;
    }
  }

  public async Task<Result<UserPrincipal>> Create(string id, UserRecord record)
  {
    try
    {
      var r = db.Users.Add(record.ToData() with { Id = id });
      await db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (UniqueConstraintException e)
    {
      logger.LogError(
        e,
        "Failed to create User due to conflicting with existing record for JWT sub '{Sub}': {@Record}",
        id,
        record.ToJson()
      );
      return new AlreadyExistException(
        "Failed to create User due to conflicting with existing record",
        e,
        typeof(UserPrincipal)
      );
    }
    catch (Exception e)
    {
      logger.LogError(
        e,
        "Failed to create User for JWT sub '{Sub}': {@Record}",
        id,
        record.ToJson()
      );
      return e;
    }
  }

  public async Task<Result<UserPrincipal?>> Update(string id, UserRecord v2)
  {
    try
    {
      var v1 = await db.Users.Where(x => x.Id == id).FirstOrDefaultAsync();
      if (v1 == null)
        return (UserPrincipal?)null;

      var v3 = v2.ToData() with { Id = id };
      var updated = db.Users.Update(v3);
      await db.SaveChangesAsync();
      return updated.Entity.ToPrincipal();
    }
    catch (UniqueConstraintException e)
    {
      logger.LogError(
        e,
        "Failed to update User due to conflicting with existing record for JWT sub '{Sub}': {@Record}",
        id,
        v2.ToJson()
      );
      return new AlreadyExistException(
        "Failed to update User due to conflicting with existing record",
        e,
        typeof(UserPrincipal)
      );
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to update User for JWT sub '{Sub}': {@Record}", id, v2.ToJson());
      return e;
    }
  }

  public async Task<Result<Unit?>> Delete(string id)
  {
    try
    {
      var a = await db.Users.Where(x => x.Id == id).FirstOrDefaultAsync();
      if (a == null)
        return (Unit?)null;

      db.Users.Remove(a);
      await db.SaveChangesAsync();
      return new Unit();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to delete User record with ID '{Id}", id);
      return e;
    }
  }
}
