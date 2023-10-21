using CSharp_Result;
using Domain.Model;
using Domain.Repository;

namespace Domain.Service;

public class TokenService : ITokenService
{
  private readonly ITokenRepository _repo;
  private readonly IApiKeyGenerator _apiKeyGenerator;

  public TokenService(ITokenRepository repo, IApiKeyGenerator apiKeyGenerator)
  {
    this._repo = repo;
    this._apiKeyGenerator = apiKeyGenerator;
  }

  public Task<Result<IEnumerable<TokenPrincipal>>> Search(string userId)
  {
    return this._repo.Search(userId);
  }

  public Task<Result<Token?>> Get(string userId, Guid id)
  {
    return this._repo.Get(userId, id);
  }

  public Task<Result<TokenPrincipal>> Create(string userId, TokenRecord record)
  {
    // generate super secret secret
    var token = this._apiKeyGenerator.Generate();
    return this._repo.Create(userId, token, record);
  }

  public Task<Result<TokenPrincipal?>> Update(string userId, Guid tokenId, TokenRecord record)
  {
    return this._repo.Update(userId, tokenId, record);
  }

  public Task<Result<Unit?>> Revoke(string userId, Guid id)
  {
    return this._repo.Revoke(userId, id);
  }

  public Task<Result<UserPrincipal?>> Validate(string token)
  {
    return this._repo.Validate(token);
  }

  public Task<Result<Unit?>> Delete(string userId, Guid id)
  {
    return this._repo.Delete(userId, id);
  }
}
