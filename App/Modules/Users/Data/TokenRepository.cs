using App.StartUp.Database;
using App.Utility;
using CSharp_Result;
using Domain.Model;
using Domain.Repository;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.Users.Data;

public class TokenRepository(MainDbContext db, ILogger<TokenRepository> logger) : ITokenRepository
{
  public async Task<Result<IEnumerable<TokenPrincipal>>> Search(string userId)
  {
    try
    {
      logger.LogInformation("Getting all token for user '{UserId}'", userId);
      var tokens = await db
        .Tokens
        .Where(x => x.UserId == userId && !x.Revoked)
        .ToArrayAsync();
      return tokens.Select(x => x.ToPrincipal()).ToResult();
    }
    catch (Exception e)
    {
      logger
        .LogError(e, "Failed getting all token for user '{UserId}'", userId);
      return e;
    }
  }

  public async Task<Result<Token?>> Get(string userId, Guid id)
  {
    try
    {
      logger.LogInformation("Retrieving Token for User '{User}' with ID '{Id}'", userId, id);
      var user = await db.Tokens
        .Where(x => x.UserId == userId && x.Id == id)
        .Include(x => x.User)
        .FirstOrDefaultAsync();
      return user?.ToDomain();
    }
    catch (Exception e)
    {
      logger
        .LogError(e, "Failed retrieving Token for User '{User}' with ID '{Id}'", userId, id);
      return e;
    }
  }

  public async Task<Result<TokenPrincipal>> Create(string userId, string token, TokenRecord record)
  {
    try
    {
      var data = record.ToData() with
      {
        UserId = userId,
        Revoked = false,
        ApiToken = token,
        User = null!,
      };
      logger.LogInformation("Creating token for User '{UserId}' with record {@Record}", userId, data.ToJson());
      var r = db.Tokens.Add(data);
      await db.SaveChangesAsync();
      return r.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to create token for User '{UserId}' with record {@Record}", userId,
        record.ToJson());
      return e;
    }
  }

  public async Task<Result<TokenPrincipal?>> Update(string userId, Guid tokenId, TokenRecord v2)
  {
    try
    {
      var v1 = await db.Tokens
        .Where(x => x.Id == tokenId && x.UserId == userId && !x.Revoked)
        .FirstOrDefaultAsync();
      if (v1 == null) return (TokenPrincipal?)null;

      var v3 = v2.ToData() with
      {
        Id = tokenId,
        UserId = userId,
        Revoked = false,
        ApiToken = v1.ApiToken,
        User = null!,
      };
      var updated = db.Tokens.Update(v3);
      await db.SaveChangesAsync();
      return updated.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to update Token  with User ID '{UserID}' and Token ID '{TokenID}': {@Record}",
        userId, tokenId, v2.ToJson());
      return e;
    }
  }

  public async Task<Result<Unit?>> Revoke(string userId, Guid tokenId)
  {
    try
    {
      var v1 = await db.Tokens
        .Where(x => x.Id == tokenId && x.UserId == userId && !x.Revoked)
        .FirstOrDefaultAsync();
      if (v1 == null) return (Unit?)null;

      var v2 = v1 with { Revoked = true, User = null! };
      var updated = db.Tokens.Update(v2);
      await db.SaveChangesAsync();
      return new Unit();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to revoke Token with User ID '{UserID}' and Token ID '{TokenID}'",
        userId, tokenId);
      return e;
    }
  }

  public async Task<Result<UserPrincipal?>> Validate(string token)
  {
    try
    {
      var a = await db.Tokens
        .Where(x => x.ApiToken == token && !x.Revoked)
        .Include(x => x.User)
        .FirstOrDefaultAsync();
      return a?.User?.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to validate token");
      return e;
    }
  }

  public async Task<Result<Unit?>> Delete(string userId, Guid tokenId)
  {
    try
    {
      var a = await db.Tokens
        .Where(x => x.Id == tokenId && x.UserId == userId && !x.Revoked)
        .FirstOrDefaultAsync();
      if (a == null) return (Unit?)null;

      db.Tokens.Remove(a);
      await db.SaveChangesAsync();
      return new Unit();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to delete Token '{TokenId}' for User '{UserId}", userId, tokenId);
      return e;
    }
  }
}
