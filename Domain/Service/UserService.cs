using CSharp_Result;
using Domain.Repository;

namespace Domain.Service;

public class UserService : IUserService
{
  private readonly IUserRepository _repo;

  public UserService(IUserRepository repo)
  {
    this._repo = repo;
  }

  public Task<Result<IEnumerable<UserPrincipal>>> Search(UserSearch search)
  {
    return this._repo.Search(search);
  }

  public Task<Result<UserPrincipal?>> GetById(Guid id)
  {
    return this._repo.GetById(id);
  }

  public Task<Result<UserPrincipal?>> GetBySub(string sub)
  {
    return this._repo.GetBySub(sub);
  }

  public Task<Result<UserPrincipal?>> GetByUsername(string username)
  {
    return this._repo.GetByUsername(username);
  }

  public Task<Result<bool>> Exists(string username)
  {
    return this._repo.Exists(username);
  }

  public Task<Result<UserPrincipal>> Create(string sub, UserRecord record)
  {
    return this._repo.Create(sub, record);
  }

  public Task<Result<UserPrincipal?>> Update(Guid id, string sub, UserRecord record)
  {
    return this._repo.Update(id, sub, record);
  }

  public Task<Result<Unit?>> Delete(Guid id)
  {
    return this._repo.Delete(id);
  }
}
