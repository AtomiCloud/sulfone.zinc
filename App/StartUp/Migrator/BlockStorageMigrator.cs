using App.StartUp.BlockStorage;
using App.StartUp.Options;
using App.Utility;
using CSharp_Result;
using Microsoft.Extensions.Options;
using Minio;

namespace App.StartUp.Migrator;

public class BlockStorageMigrator
{
  private readonly ILogger<BlockStorageMigrator> _logger;
  private readonly IBlockStorageFactory _factory;
  private readonly IOptionsMonitor<Dictionary<string, BlockStorageOption>> _store;

  public BlockStorageMigrator(ILogger<BlockStorageMigrator> logger, IBlockStorageFactory factory, IOptionsMonitor<Dictionary<string, BlockStorageOption>> store)
  {
    this._logger = logger;
    this._factory = factory;
    this._store = store;
  }

  public async Task<Result<IEnumerable<Unit>>> Migrate()
  {
    var result = await this._store.CurrentValue
      .Select(x => this.MigrateBlockStorage(x.Key, x.Value))
      .AwaitAll();
    return result.ToResultOfSeq();
  }
  private async Task<Result<Unit>> MigrateBlockStorage(string key, BlockStorageOption o)
  {
    if (!o.EnsureBucketCreation)
    {
      this._logger.LogInformation("Bucket check skipped: {BlockStorageName}", key);
      return new Unit();
    }

    using var scope = this._logger.BeginScope("Check BlockStorage: {BlockStorageName}", key);
    try
    {
      var b = this._factory.Get(key);

      this._logger.LogInformation("Checking bucket for: {BlockStorageName}", key);
      var beArgs = new BucketExistsArgs().WithBucket(o.Bucket);
      var found = await b.Client.BucketExistsAsync(beArgs);
      this._logger.LogInformation("Bucket {BucketName} Exist: {BucketExist}", key, found);
      if (found) return new Unit();

      var mbArgs = new MakeBucketArgs().WithBucket(o.Bucket);
      await b.Client.MakeBucketAsync(mbArgs);
      this._logger.LogInformation("Bucket created: {BlockStorageName}", key);
      if (o.Policy != "Public") return new Unit();
      var policy = $$"""
  {
    "Version": "2012-10-17",
    "Statement": [
      {
        "Effect": "Allow",
        "Principal": {
          "AWS": [
            "*"
          ]
        },
        "Action": [
          "s3:GetObject"
        ],
        "Resource": [
          "arn:aws:s3:::{{o.Bucket}}/*"
        ]
      }
    ]
  }
  """;
      var spa = new SetPolicyArgs()
        .WithBucket(o.Bucket)
        .WithPolicy(policy);

      await b.Client.SetPolicyAsync(spa).ConfigureAwait(false);
      this._logger.LogInformation("Configured Bucket: {BlockStorageName}", key);
      return new Unit();
    }
    catch (Exception e)
    {
      this._logger.LogCritical(e, "Bucket check failed: {Message}", e.Message);
      return e;
    }
  }
}
