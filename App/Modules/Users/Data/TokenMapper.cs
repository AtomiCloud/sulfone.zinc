using Domain.Model;

namespace App.Modules.Users.Data;

public static class TokenMapper
{
  public static TokenRecord ToRecord(this TokenData data) =>
    new() { Name = data.Name, Revoked = data.Revoked };

  public static TokenProp ToProp(this TokenData data) => new() { ApiToken = data.ApiToken };

  public static TokenPrincipal ToPrincipal(this TokenData data) =>
    new()
    {
      Id = data.Id,
      Record = data.ToRecord(),
      Property = data.ToProp(),
    };

  public static Token ToDomain(this TokenData data) =>
    new() { Principal = data.ToPrincipal(), UserPrincipal = data.User.ToPrincipal() };

  public static TokenData ToData(this TokenRecord record) =>
    new() { Name = record.Name, Revoked = record.Revoked };
}
