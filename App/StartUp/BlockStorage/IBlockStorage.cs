using Minio;

namespace App.StartUp.BlockStorage;

public interface IBlockStorage
{
  MinioClient Client { get; }

  string Bucket { get; }

  public string Scheme { get; }

  public string Host { get; }
  public int Port { get; }
}
