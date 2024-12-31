using System.Security.Claims;
using System.Text.Encodings.Web;
using App.StartUp.Database;
using Domain.Repository;
using Domain.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace App.Modules.Users.API.Auth;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
  public const string DefaultScheme = "ApiKeyTokenScheme";
  public string TokenHeaderName { get; set; } = "X-API-TOKEN";
}

public class ApiKeyAuthenticationHandler(
  IOptionsMonitor<ApiKeyAuthenticationOptions> options,
  ILoggerFactory logger,
  UrlEncoder encoder,
  ITokenService token)
  : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
  protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    //check header first
    if (!this.Request.Headers.TryGetValue(this.Options.TokenHeaderName, out var value))
    {
      return AuthenticateResult.Fail($"Missing header: {this.Options.TokenHeaderName}");
    }

    //get the header and validate
    string token1 = value!;

    var uR = await token.Validate(token1);
    if (uR.IsFailure()) return AuthenticateResult.Fail("Invalid token");

    var u = uR.Get();
    if (u == null) return AuthenticateResult.Fail("Invalid token");

    var id = u.Id;
    var name = u.Record.Username;
    //Success! Add details here that identifies the user
    var claims = new List<Claim> { new("sub", id), new("username", name) };

    var claimsIdentity = new ClaimsIdentity
      (claims, this.Scheme.Name, "sub", ClaimTypes.Role);
    var claimsPrincipal = new ClaimsPrincipal
      (claimsIdentity);
    return AuthenticateResult.Success
      (new AuthenticationTicket(claimsPrincipal, this.Scheme.Name));
  }
}
