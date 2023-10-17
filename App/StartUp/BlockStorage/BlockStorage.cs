using Minio;

namespace App.StartUp.BlockStorage;

public class BlockStorage : IBlockStorage
{
  public BlockStorage(MinioClient client, string bucket, string scheme, string host, int port)
  {
    this.Client = client;
    this.Bucket = bucket;
    this.Scheme = scheme;
    this.Host = host;
    this.Port = port;
  }

  public MinioClient Client { get; }
  public string Bucket { get; }

  public string Scheme { get; }

  public string Host { get; }
  public int Port { get; }
}
