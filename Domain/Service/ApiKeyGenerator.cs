using PasswordGenerator;

namespace Domain.Service;

public class ApiKeyGenerator : IApiKeyGenerator
{
  public string Generate()
  {
    var pwd = new Password()
      .IncludeLowercase()
      .IncludeNumeric()
      .IncludeUppercase();
    return pwd.Next();
  }
}
