using CSharp_Result;
using Domain.Model;
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

  public Task<Result<User?>> GetById(string id)
  {
    return this._repo.GetById(id);
  }

  public Task<Result<User?>> GetByUsername(string username)
  {
    return this._repo.GetByUsername(username);
  }

  public Task<Result<bool>> Exists(string username)
  {
    return this._repo.Exists(username);
  }

  public Task<Result<UserPrincipal>> Create(string id, UserRecord record)
  {
    return this._repo.Create(id, record);
  }

  public Task<Result<UserPrincipal?>> Update(string id, UserRecord record)
  {
    return this._repo.Update(id, record);
  }

  public Task<Result<Unit?>> Delete(string id)
  {
    return this._repo.Delete(id);
  }
}
