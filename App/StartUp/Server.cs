using System.Reflection;
using App.Modules;
using App.Modules.Common;
using App.StartUp.BlockStorage;
using App.StartUp.Database;
using App.StartUp.Migrator;
using App.StartUp.Options;
using App.StartUp.Options.Auth;
using App.StartUp.Options.Logging;
using App.StartUp.Options.Metrics;
using App.StartUp.Options.Traces;
using App.StartUp.Services;
using App.StartUp.Services.Swagger;
using App.Utility;
using CSharp_Result;
using FluentValidation;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using Utils = App.Utility.Utils;

namespace App.StartUp;

public class Server(
  IOptionsMonitor<List<CorsOption>> cors,
  IOptionsMonitor<AppOption> app,
  IOptionsMonitor<MetricOption> metrics,
  IOptionsMonitor<LogsOption> logs,
  IOptionsMonitor<TraceOption> trace,
  IOptionsMonitor<ErrorPortalOption> errorPortal,
  IOptionsMonitor<Dictionary<string, BlockStorageOption>> store,
  IOptionsMonitor<Dictionary<string, HttpClientOption>> http,
  IOptionsMonitor<AuthOption> auth,
  IOptionsMonitor<Dictionary<string, CacheOption>> cache
)
{
  private void ConfigureResourceBuilder(ResourceBuilder r)
  {
    var a = app.CurrentValue;
    r.AddService(serviceName: $"{a.Platform}.{a.Service}.{a.Module}")
      .AddAttributes(
        new KeyValuePair<string, object>[]
        {
          new("atomicloud.landscape", a.Landscape),
          new("atomicloud.platform", a.Platform),
          new("atomicloud.service", a.Service),
          new("atomicloud.module", a.Module),
          new("atomicloud.version", a.Version),
          new("atomicloud.template", "dotnet"),
          new("atomicloud.execution_mode", a.Mode),
        }
      );
    ;
  }

  public void Start(string landscape, string[] args)
  {
    var meter = $"{app.CurrentValue.Platform}.{app.CurrentValue.Service}.{app.CurrentValue.Module}";

    var builder = WebApplication.CreateBuilder(args);

    builder
      .Configuration.AddYamlFile("Config/settings.yaml", optional: false, reloadOnChange: true)
      .AddYamlFile($"Config/settings.{landscape}.yaml", optional: true, reloadOnChange: true)
      .AddEnvironmentVariables(prefix: "Atomi_");

    // builder.Logging.ClearProviders();
    builder.Logging.AddOpenTelemetry(o =>
    {
      var b = ResourceBuilder.CreateDefault();
      this.ConfigureResourceBuilder(b);
      o.SetResourceBuilder(b);
      o.AddLogsService(logs);
    });

    var services = builder.Services;

    services.AddStartupOptions();

    // Allow for ContentInspector to be available.
    services.AddMimeDetectionService().AddScoped<IFileValidator, FileValidator>();

    services.AddSingleton(new Instrumentation(app, meter));

    services
      .AddProblemDetailsService(errorPortal.CurrentValue, app.CurrentValue)
      .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    services.AddControllers();

    services.AddEndpointsApiExplorer();
    services
      .AddApiVersioning(options =>
      {
        options.ReportApiVersions = true;
      })
      .AddApiExplorer(options =>
      {
        options.GroupNameFormat = "'v'VVV";
      });

    services.AddSwaggerService(auth.CurrentValue);

    // Http Client
    services.AddHttpClientService(http.CurrentValue);

    // Cors Services
    services.AddCorsService(cors.CurrentValue);

    // OTEL configuration
    services
      .AddOpenTelemetry()
      .ConfigureResource(this.ConfigureResourceBuilder)
      .AddMetricService(metrics, meter)
      .AddTraceService(trace);

    // Database Configurations
    services
      .AddSingleton<DatabaseMigrator>()
      .AddDbContext<MainDbContext>()
      .AddHostedService<DbMigratorHostedService>();

    // Cache Configurations
    services.AddCache(cache.CurrentValue);

    // Block Storage Configuration
    services
      .AddBlockStorage(store.CurrentValue)
      .AddSingleton<BlockStorageMigrator>()
      .AddHostedService<BlockStorageHostedService>()
      .AddTransient<IFileRepository, FileRepository>();

    // Auth Service Configuration
    if (auth.CurrentValue.Enabled)
      services.AddAuthService(auth.CurrentValue);

    services.AddDomainServices();
    /*----------------------------------------*/
    // Pipeline
    var app1 = builder.Build();

    if (app.CurrentValue.GenerateConfig)
      File.WriteAllText("Config/schema.json", Utils.OptionSchema.ActualSchema.ToJson());

    switch (app.CurrentValue.Mode)
    {
      case "Migration":
        app1.Logger.LogInformation("Starting in Migration Mode...");
        this.StartMigration(app1).Wait();
        break;
      case "Server":
        app1.Logger.LogInformation("Starting in Server Mode...");
        this.StartServer(app1);
        break;
      default:
        throw new ApplicationException($"Unknown mode: {app.CurrentValue.Mode}");
    }
  }

  private async Task StartMigration(WebApplication app)
  {
    var m1 = app.Services.GetService<DatabaseMigrator>();
    var m2 = app.Services.GetService<BlockStorageMigrator>();

    var t1 = m1?.Migrate();
    var t2 = m2?.Migrate();
    if (t1 is null || t2 is null)
    {
      var ex = new ApplicationException("Migrators not found");
      app.Logger.LogCritical(
        ex,
        "Migrators not resolved: {M1}, {M2}",
        t1 is not null,
        t2 is not null
      );
      throw ex;
    }

    var all = await Task.WhenAll(t1, t2);

    var a = all.ToResultOfSeq().Then(x => x.SelectMany(m => m), Errors.MapAll);
    if (a.IsSuccess())
    {
      app.Logger.LogInformation("Migrations completed successfully!");
    }
    else
    {
      var ex = a.FailureOrDefault();
      app.Logger.LogCritical(ex, "Failed to migrate");
      throw ex;
    }
  }

  private void StartServer(WebApplication app1)
  {
    using (
      app1.Logger.BeginScope(
        new List<KeyValuePair<string, object>>
        {
          new("app", app.CurrentValue.ToJson()),
          new("metrics", metrics.CurrentValue.ToJson()),
          new("logs", logs.CurrentValue.ToJson()),
          new("traces", trace.CurrentValue.ToJson()),
          new("cors", cors.CurrentValue.ToJson()),
        }
      )
    )
    {
      app1.Logger.LogInformation("Configurations");
    }

    app1.UseExceptionHandler();

    if (errorPortal.CurrentValue.EnableExceptionResponse)
      app1.UseDeveloperExceptionPage();

    if (app.CurrentValue.EnableSwagger)
      app1.UseSwaggerService();
    app1.UseCors(app.CurrentValue.DefaultCors);

    if (auth.CurrentValue.Enabled)
      app1.UseAuthService();
    app1.MapControllers();
    app1.Run();
  }
}
