using App.StartUp.BlockStorage;
using App.StartUp.Options;
using Minio;

namespace App.StartUp.Services;

public static class BlockStorageService
{
  public static IServiceCollection AddBlockStorage(this IServiceCollection services,
    Dictionary<string, BlockStorageOption> o)
  {
    var s = new BlockStorageFactory();
    services.AddSingleton<IBlockStorageFactory>((sp) => s)
      .AutoTrace<IBlockStorageFactory>();
    foreach (var (k, v) in o)
    {
      var mc = new MinioClient()
        .WithEndpoint($"{v.Host}:{v.Port}")
        .WithCredentials(v.AccessKey, v.SecretKey)
        .WithSSL(v.UseSSL)
        .Build();
      var m = mc ?? throw new ApplicationException($"Minio client is null: {k}");
      var b = new BlockStorage.BlockStorage(m, v.Bucket, v.Scheme, v.Host, v.Port);
      s.Add(k, b);
    }

    return services;
  }
}
