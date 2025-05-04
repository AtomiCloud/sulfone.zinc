using Domain;
using Domain.Model;

namespace App.Modules.Users.Data;

public static class UserMapper
{
  public static UserRecord ToRecord(this UserData principal) =>
    new() { Username = principal.Username };

  public static UserPrincipal ToPrincipal(this UserData data) =>
    new() { Id = data.Id, Record = data.ToRecord() };

  public static User ToDomain(this UserData data) =>
    new() { Principal = data.ToPrincipal(), Tokens = data.Tokens.Select(x => x.ToPrincipal()) };

  public static UserData ToData(this UserRecord record) => new() { Username = record.Username };
}
