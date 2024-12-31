using CSharp_Result;
using Domain.Model;
using Domain.Repository;

namespace Domain.Service;

public class TokenService(ITokenRepository repo, IApiKeyGenerator apiKeyGenerator) : ITokenService
{
  public Task<Result<IEnumerable<TokenPrincipal>>> Search(string userId)
  {
    return repo.Search(userId);
  }

  public Task<Result<Token?>> Get(string userId, Guid id)
  {
    return repo.Get(userId, id);
  }

  public Task<Result<TokenPrincipal>> Create(string userId, TokenRecord record)
  {
    // generate super secret secret
    var token = apiKeyGenerator.Generate();
    return repo.Create(userId, token, record);
  }

  public Task<Result<TokenPrincipal?>> Update(string userId, Guid tokenId, TokenRecord record)
  {
    return repo.Update(userId, tokenId, record);
  }

  public Task<Result<Unit?>> Revoke(string userId, Guid id)
  {
    return repo.Revoke(userId, id);
  }

  public Task<Result<UserPrincipal?>> Validate(string token)
  {
    return repo.Validate(token);
  }

  public Task<Result<Unit?>> Delete(string userId, Guid id)
  {
    return repo.Delete(userId, id);
  }
}
