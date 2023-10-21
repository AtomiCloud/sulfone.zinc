
using App.Modules.Users.Data;
using App.StartUp.Services;
using App.Utility;
using Domain.Repository;
using Domain.Service;

namespace App.Modules;

public static class DomainServices
{
  public static IServiceCollection AddDomainServices(this IServiceCollection s)
  {
    s.AddScoped<IUserService, UserService>()
      .AutoTrace<IUserService>();

    s.AddScoped<IUserRepository, UserRepository>()
      .AutoTrace<IUserRepository>();

    s.AddScoped<ITokenRepository, TokenRepository>()
      .AutoTrace<ITokenRepository>();

    s.AddScoped<IApiKeyGenerator, ApiKeyGenerator>()
      .AutoTrace<IApiKeyGenerator>();

    s.AddScoped<ITokenService, TokenService>()
      .AutoTrace<ITokenService>();




    return s;
  }
}
