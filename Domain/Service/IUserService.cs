using CSharp_Result;
using Domain.Model;

namespace Domain.Service;

public interface IUserService
{

  Task<Result<IEnumerable<UserPrincipal>>> Search(UserSearch search);
  Task<Result<User?>> GetById(string id);
  Task<Result<User?>> GetByUsername(string username);

  Task<Result<bool>> Exists(string username);

  Task<Result<UserPrincipal>> Create(string id, UserRecord record);
  Task<Result<UserPrincipal?>> Update(string id, UserRecord record);

  Task<Result<Unit?>> Delete(string id);
}
