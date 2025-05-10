using App.StartUp.Migrator;

namespace App.StartUp.Database;

public class DbMigratorHostedService(
  DatabaseMigrator databaseMigrator,
  ILogger<DbMigratorHostedService> logger,
  IHostApplicationLifetime lifetime
) : IHostedService
{
  public async Task StartAsync(CancellationToken cancellationToken)
  {
    var r = await databaseMigrator.Migrate();
    if (r.IsFailure())
    {
      var e = r.FailureOrDefault();
      logger.LogCritical(e, "Failed to migrate or contact databases");
      Environment.ExitCode = 1;
      lifetime.StopApplication();
    }
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}
