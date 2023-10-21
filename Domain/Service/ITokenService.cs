using CSharp_Result;
using Domain.Model;

namespace Domain.Service;

public interface ITokenService
{
  Task<Result<IEnumerable<TokenPrincipal>>> Search(string userId);

  Task<Result<Token?>> Get(string userId, Guid id);

  Task<Result<TokenPrincipal>> Create(string userId, TokenRecord record);

  Task<Result<TokenPrincipal?>> Update(string userId, Guid tokenId, TokenRecord record);

  Task<Result<Unit?>> Revoke(string userId, Guid id);

  Task<Result<UserPrincipal?>> Validate(string token);

  Task<Result<Unit?>> Delete(string userId, Guid id);
}
