using Domain;

namespace App.Modules.Users.API.V1;

public static class Mapper
{
  public static UserResp ToResp(this UserPrincipal userPrincipal)
  {
    return new(userPrincipal.Id, userPrincipal.Sub, userPrincipal.Record.Username);
  }

  public static UserRecord ToRecord(this CreateUserReq req)
  {
    return new UserRecord
    {
      Username = req.Username,
    };
  }

  public static UserRecord ToRecord(this UpdateUserReq req)
  {
    return new UserRecord
    {
      Username = req.Username,
    };
  }

  public static UserSearch ToDomain(this SearchUserQuery query)
  {
    return new UserSearch
    {
      Id = query.Id,
      Sub = query.Sub,
      Username = query.Username,
      Limit = query.Limit ?? 20,
      Skip = query.Skip ?? 0,
    };
  }

}
