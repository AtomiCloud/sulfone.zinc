using Minio;

namespace App.StartUp.BlockStorage;

public class BlockStorage(MinioClient client, string bucket, string scheme, string host, int port)
  : IBlockStorage
{
  public MinioClient Client { get; } = client;
  public string Bucket { get; } = bucket;

  public string Scheme { get; } = scheme;

  public string Host { get; } = host;
  public int Port { get; } = port;
}
