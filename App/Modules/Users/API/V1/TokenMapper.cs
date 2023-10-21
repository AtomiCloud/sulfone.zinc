using CSharp_Result;
using Domain.Model;

namespace App.Modules.Users.API.V1;

public static class TokenMapper
{
  public static TokenRecord ToRecord(this CreateTokenReq req) =>
    new() { Name = req.Name, Revoked = false, };

  public static TokenRecord ToRecord(this UpdateTokenReq req) =>
    new() { Name = req.Name, Revoked = false, };

  public static TokenPrincipalResp ToResp(this TokenPrincipal token) =>
    new(token.Id, token.Record.Name);

  public static TokenOTPrincipalResp ToOTResp(this TokenPrincipal token) =>
    new(token.Id, token.Record.Name, token.Property.ApiToken);

  public static TokenOTResp ToOTResp(this Token token) =>
    new(token.Principal.ToOTResp(), token.UserPrincipal.ToResp());

  public static TokenResp ToResp(this Token token) =>
    new(token.Principal.ToResp(), token.UserPrincipal.ToResp());
}
