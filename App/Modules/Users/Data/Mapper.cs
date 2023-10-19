using Domain;

namespace App.Modules.Users.Data;

public static class Mapper
{
  public static UserRecord ToRecord(this UserData principal) => new() { Username = principal.Username };

  public static UserPrincipal ToPrincipal(this UserData data) => new()
  {
    Id = data.Id,
    Sub = data.Sub,
    Record = data.ToRecord(),
  };

  public static UserData ToData(this UserPrincipal principal) => new()
  {
    Id = principal.Id,
    Sub = principal.Sub,
    Username = principal.Record.Username,
  };

  public static UserData ToData(this UserRecord record) => new()
  {
    Username = record.Username,
  };

}
