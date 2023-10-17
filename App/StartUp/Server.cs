using System.Reflection;
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

public class Server
{
  private readonly IOptionsMonitor<List<CorsOption>> _cors;
  private readonly IOptionsMonitor<AppOption> _app;
  private readonly IOptionsMonitor<MetricOption> _metrics;
  private readonly IOptionsMonitor<TraceOption> _trace;
  private readonly IOptionsMonitor<LogsOption> _logs;
  private readonly IOptionsMonitor<ErrorPortalOption> _errorPortal;
  private readonly IOptionsMonitor<AuthOption> _auth;
  private readonly IOptionsMonitor<Dictionary<string, CacheOption>> _cache;
  private readonly IOptionsMonitor<Dictionary<string, BlockStorageOption>> _store;
  private readonly IOptionsMonitor<Dictionary<string, HttpClientOption>> _http;

  public Server(
    IOptionsMonitor<List<CorsOption>> cors,
    IOptionsMonitor<AppOption> app,
    IOptionsMonitor<MetricOption> metrics,
    IOptionsMonitor<LogsOption> logs,
    IOptionsMonitor<TraceOption> trace, IOptionsMonitor<ErrorPortalOption> errorPortal,
    IOptionsMonitor<Dictionary<string, BlockStorageOption>> store,
    IOptionsMonitor<Dictionary<string, HttpClientOption>> http, IOptionsMonitor<AuthOption> auth, IOptionsMonitor<Dictionary<string, CacheOption>> cache)
  {
    this._cors = cors;
    this._app = app;
    this._metrics = metrics;
    this._logs = logs;
    this._trace = trace;
    this._errorPortal = errorPortal;
    this._store = store;
    this._http = http;
    this._auth = auth;
    this._cache = cache;
  }

  private void ConfigureResourceBuilder(ResourceBuilder r)
  {
    var a = this._app.CurrentValue;
    r.AddService(serviceName: $"{a.Platform}.{a.Service}.{a.Module}")
      .AddAttributes(new KeyValuePair<string, object>[]
      {
        new("atomicloud.landscape", a.Landscape), new("atomicloud.platform", a.Platform),
        new("atomicloud.service", a.Service), new("atomicloud.module", a.Module),
        new("atomicloud.version", a.Version), new("atomicloud.template", "dotnet"),
        new("atomicloud.execution_mode", a.Mode),
      });
    ;
  }


  public void Start(string landscape, string[] args)
  {
    var meter = $"{this._app.CurrentValue.Platform}.{this._app.CurrentValue.Service}.{this._app.CurrentValue.Module}";

    var builder = WebApplication
      .CreateBuilder(args);

    builder.Configuration
      .AddYamlFile("Config/settings.yaml", optional: false, reloadOnChange: true)
      .AddYamlFile($"Config/settings.{landscape}.yaml", optional: true, reloadOnChange: true)
      .AddEnvironmentVariables(prefix: "Atomi_");

    // builder.Logging.ClearProviders();
    builder.Logging.AddOpenTelemetry(
      o =>
      {
        var b = ResourceBuilder.CreateDefault();
        this.ConfigureResourceBuilder(b);
        o.SetResourceBuilder(b);
        o.AddLogsService(this._logs);
      });

    var services = builder.Services;

    services.AddStartupOptions();

    // Allow for ContentInspector to be available.
    services.AddMimeDetectionService()
      .AddScoped<IFileValidator, FileValidator>();

    services.AddSingleton(new Instrumentation(this._app, meter));

    services.AddProblemDetailsService(this._errorPortal.CurrentValue, this._app.CurrentValue)
      .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    services.AddControllers();

    services.AddEndpointsApiExplorer();
    services.AddApiVersioning(options =>
      {
        options.ReportApiVersions = true;
      })
      .AddApiExplorer(options =>
      {
        options.GroupNameFormat = "'v'VVV";
      });

    services.AddSwaggerService(this._auth.CurrentValue);

    // Http Client
    services.AddHttpClientService(this._http.CurrentValue);

    // Cors Services
    services.AddCorsService(this._cors.CurrentValue);

    // OTEL configuration
    services
      .AddOpenTelemetry()
      .ConfigureResource(this.ConfigureResourceBuilder)
      .AddMetricService(this._metrics, meter)
      .AddTraceService(this._trace);

    // Database Configurations
    services
      .AddSingleton<DatabaseMigrator>()
      .AddDbContext<MainDbContext>()
      .AddHostedService<DbMigratorHostedService>();

    // Cache Configurations
    services
      .AddCache(this._cache.CurrentValue);

    // Block Storage Configuration
    services
      .AddBlockStorage(this._store.CurrentValue)
      .AddSingleton<BlockStorageMigrator>()
      .AddHostedService<BlockStorageHostedService>()
      .AddTransient<IFileRepository, FileRepository>();

    // Auth Service Configuration
    if (this._auth.CurrentValue.Enabled)
      services.AddAuthService(this._auth.CurrentValue);


    /*----------------------------------------*/
    // Pipeline
    var app = builder.Build();

    if (this._app.CurrentValue.GenerateConfig) File.WriteAllText("Config/schema.json", Utils.OptionSchema.ActualSchema.ToJson());


    switch (this._app.CurrentValue.Mode)
    {
      case "Migration":
        app.Logger.LogInformation("Starting in Migration Mode...");
        this.StartMigration(app).Wait();
        break;
      case "Server":
        app.Logger.LogInformation("Starting in Server Mode...");
        this.StartServer(app);
        break;
      default:
        throw new ApplicationException($"Unknown mode: {this._app.CurrentValue.Mode}");
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
      app.Logger.LogCritical(ex, "Migrators not resolved: {M1}, {M2}",
        t1 is not null, t2 is not null);
      throw ex;
    }

    var all = await Task.WhenAll(t1, t2);

    var a = all.ToResultOfSeq()
      .Then(x => x.SelectMany(m => m), Errors.MapAll);
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

  private void StartServer(WebApplication app)
  {
    using (app.Logger.BeginScope(new List<KeyValuePair<string, object>>
           {
             new("app", this._app.CurrentValue.ToJson()),
             new("metrics", this._metrics.CurrentValue.ToJson()),
             new("logs", this._logs.CurrentValue.ToJson()),
             new("traces", this._trace.CurrentValue.ToJson()),
             new("cors", this._cors.CurrentValue.ToJson()),
           }))
    {
      app.Logger.LogInformation("Configurations");
    }

    app.UseExceptionHandler();

    if (this._errorPortal.CurrentValue.EnableExceptionResponse) app.UseDeveloperExceptionPage();


    if (this._app.CurrentValue.EnableSwagger) app.UseSwaggerService();
    app.UseCors(this._app.CurrentValue.DefaultCors);

    if (this._auth.CurrentValue.Enabled) app.UseAuthService();
    app.MapControllers();
    app.Run();
  }
}
