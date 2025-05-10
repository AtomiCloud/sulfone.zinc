using Domain.Model;

namespace App.Modules.Users.API.V1;

public static class UserMapper
{
  public static UserPrincipalResp ToResp(this UserPrincipal userPrincipal) =>
    new(userPrincipal.Id, userPrincipal.Record.Username);

  public static UserResp ToResp(this User user) =>
    new(user.Principal.ToResp(), user.Tokens.Where(x => !x.Record.Revoked).Select(x => x.ToResp()));

  public static UserRecord ToRecord(this CreateUserReq req)
  {
    return new UserRecord { Username = req.Username };
  }

  public static UserRecord ToRecord(this UpdateUserReq req)
  {
    return new UserRecord { Username = req.Username };
  }

  public static UserSearch ToDomain(this SearchUserQuery query)
  {
    return new UserSearch
    {
      Id = query.Id,
      Username = query.Username,
      Limit = query.Limit ?? 20,
      Skip = query.Skip ?? 0,
    };
  }
}
