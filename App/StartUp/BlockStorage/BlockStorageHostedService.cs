using App.StartUp.Migrator;

namespace App.StartUp.BlockStorage;

public class BlockStorageHostedService : IHostedService
{
  private readonly BlockStorageMigrator _blockStorageMigrator;
  private readonly ILogger<BlockStorageHostedService> _logger;
  private readonly IHostApplicationLifetime _lifetime;


  public BlockStorageHostedService(BlockStorageMigrator blockStorageMigrator, ILogger<BlockStorageHostedService> logger,
    IHostApplicationLifetime lifetime)
  {
    this._blockStorageMigrator = blockStorageMigrator;
    this._logger = logger;
    this._lifetime = lifetime;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    var r = await this._blockStorageMigrator.Migrate();
    if (r.IsFailure())
    {
      var e = r.FailureOrDefault();
      this._logger.LogCritical(e, "Failed to migrate or contact block storage");
      Environment.ExitCode = 1;
      this._lifetime.StopApplication();
    }
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}
