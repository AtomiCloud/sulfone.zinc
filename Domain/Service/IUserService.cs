using CSharp_Result;

namespace Domain.Service;

public interface IUserService
{
  Task<Result<IEnumerable<UserPrincipal>>> Search(UserSearch search);
  Task<Result<UserPrincipal?>> GetById(Guid id);
  Task<Result<UserPrincipal?>> GetBySub(string sub);
  Task<Result<UserPrincipal?>> GetByUsername(string username);

  Task<Result<bool>> Exists(string username);

  Task<Result<UserPrincipal>> Create(string sub, UserRecord record);
  Task<Result<UserPrincipal?>> Update(Guid id, string sub, UserRecord record);

  Task<Result<Unit?>> Delete(Guid id);
}
