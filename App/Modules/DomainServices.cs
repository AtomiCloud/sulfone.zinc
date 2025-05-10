using App.Modules.Cyan.Data.Repositories;
using App.Modules.Users.Data;
using App.StartUp.Services;
using Domain.Repository;
using Domain.Service;

namespace App.Modules;

public static class DomainServices
{
  public static IServiceCollection AddDomainServices(this IServiceCollection s)
  {
    s.AddScoped<IUserService, UserService>().AutoTrace<IUserService>();

    s.AddScoped<IUserRepository, UserRepository>().AutoTrace<IUserRepository>();

    s.AddScoped<ITokenRepository, TokenRepository>().AutoTrace<ITokenRepository>();

    s.AddScoped<IApiKeyGenerator, ApiKeyGenerator>().AutoTrace<IApiKeyGenerator>();

    s.AddScoped<ITokenService, TokenService>().AutoTrace<ITokenService>();

    s.AddScoped<IPluginService, PluginService>().AutoTrace<IPluginService>();

    s.AddScoped<IPluginRepository, PluginRepository>().AutoTrace<IPluginRepository>();

    s.AddScoped<IProcessorService, ProcessorService>().AutoTrace<IProcessorService>();

    s.AddScoped<IProcessorRepository, ProcessorRepository>().AutoTrace<IProcessorRepository>();

    s.AddScoped<ITemplateService, TemplateService>().AutoTrace<ITemplateService>();

    s.AddScoped<ITemplateRepository, TemplateRepository>().AutoTrace<ITemplateRepository>();

    return s;
  }
}
