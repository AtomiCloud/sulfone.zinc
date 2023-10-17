using App.StartUp.Migrator;
using App.StartUp.Options;
using App.StartUp.Registry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace App.StartUp.Database;

public class DbMigratorHostedService : IHostedService
{
  private readonly DatabaseMigrator _databaseMigrator;
  private readonly ILogger<DbMigratorHostedService> _logger;
  private readonly IHostApplicationLifetime _lifetime;


  public DbMigratorHostedService(DatabaseMigrator databaseMigrator, ILogger<DbMigratorHostedService> logger,
    IHostApplicationLifetime lifetime)
  {
    this._databaseMigrator = databaseMigrator;
    this._logger = logger;
    this._lifetime = lifetime;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    var r = await this._databaseMigrator.Migrate();
    if (r.IsFailure())
    {
      var e = r.FailureOrDefault();
      this._logger.LogCritical(e, "Failed to migrate or contact databases");
      Environment.ExitCode = 1;
      this._lifetime.StopApplication();
    }
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}
