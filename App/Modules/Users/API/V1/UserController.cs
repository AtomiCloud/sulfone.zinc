using System.Net.Mime;
using App.Error.V1;
using App.Modules.Common;
using App.StartUp.Registry;
using App.Utility;
using Asp.Versioning;
using CSharp_Result;
using Domain;
using Domain.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Modules.Users.API.V1;

/// <summary>
/// V1 controller
/// </summary>
[ApiVersion(1.0)]
[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Route("api/v{version:apiVersion}/[controller]")]
public class UserController : AtomiControllerBase

{
  private readonly IUserService _service;
  private readonly CreateUserReqValidator _createUserReqValidator;
  private readonly UpdateUserReqValidator _updateUserReqValidator;
  private readonly UserSearchQueryValidator _userSearchQueryValidator;


  public UserController(IUserService service,
    CreateUserReqValidator createUserReqValidator, UpdateUserReqValidator updateUserReqValidator,
    UserSearchQueryValidator userSearchQueryValidator)
  {
    this._service = service;
    this._createUserReqValidator = createUserReqValidator;
    this._updateUserReqValidator = updateUserReqValidator;
    this._userSearchQueryValidator = userSearchQueryValidator;
  }

  // Policy = AuthPolicies.OnlyAdmin
  [Authorize, HttpGet]
  public async Task<ActionResult<IEnumerable<UserResp>>> Search([FromQuery] SearchUserQuery query)
  {
    var x = await this._userSearchQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchUserQuery")
      .ThenAwait(q => this._service.Search(q.ToDomain()))
      .Then(x => x.Select(u => u.ToResp()).ToResult());
    return this.ReturnResult(x);
  }

  [Authorize, HttpGet("{id:guid}")]
  public async Task<ActionResult<UserResp>> GetById(Guid id)
  {
    var user = await this._service.GetById(id)
      .Then(x => (x?.ToResp()).ToResult())
      .Then(x =>
      {
        if (x?.Sub == this.Sub()) return x.ToResult();
        return new Unauthorized("You are not authorized to access this resource")
          .ToException();
      });
    return this.ReturnNullableResult(user, new EntityNotFound(
      "User Not Found", typeof(UserPrincipal), id.ToString()));
  }

  [Authorize, HttpGet("sub/{sub}")]
  public async Task<ActionResult<UserResp>> GetBySub(string sub)
  {
    if (this.Sub() != sub)
    {
      Result<UserResp> x = new Unauthorized("You are not authorized to access this resource").ToException();
      return this.ReturnResult(x);
    }

    var user = await this._service.GetBySub(sub)
      .Then(x => (x?.ToResp()).ToResult());
    return this.ReturnNullableResult(user, new EntityNotFound(
      "User Not Found", typeof(UserPrincipal), sub));
  }

  [Authorize, HttpGet("username/{username}")]
  public async Task<ActionResult<UserResp>> GetByUsername(string username)
  {
    var user = await this._service.GetByUsername(username)
      .Then(x => (x?.ToResp()).ToResult())
      .Then(x =>
      {
        if (x?.Sub == this.Sub()) return x.ToResult();
        return new Unauthorized("You are not authorized to access this resource")
          .ToException();
      });
    return this.ReturnNullableResult(user, new EntityNotFound(
      "User Not Found", typeof(UserPrincipal), username));
  }

  [Authorize, HttpGet("exist/{username}")]
  public async Task<ActionResult<bool>> Exist(string username)
  {
    var exist = await this._service.Exists(username);
    return this.ReturnResult(exist);
  }

  [Authorize, HttpPost]
  public async Task<ActionResult<UserResp>> Create([FromBody] CreateUserReq req)
  {
    var sub = this.Sub();
    if (sub == null)
    {
      Result<UserResp> x = new Unauthorized("You are not authorized to access this resource").ToException();
      return this.ReturnResult(x);
    }
    var user = await this._createUserReqValidator
      .ValidateAsyncResult(req, "Invalid CreateUserReq")
      .ThenAwait(x => this._service.Create(sub, x.ToRecord()))
      .Then(x => x.ToResp().ToResult());
    return this.ReturnResult(user);
  }

  [Authorize, HttpPut("{id:guid}")]
  public async Task<ActionResult<UserResp>> Update(Guid id, [FromBody] UpdateUserReq req)
  {
    var sub = this.Sub();
    if (sub == null)
    {
      Result<UserResp> x = new Unauthorized("You are not authorized to access this resource").ToException();
      return this.ReturnResult(x);
    }
    var user = await this._updateUserReqValidator
      .ValidateAsyncResult(req, "Invalid UpdateUserReq")
      .ThenAwait(x => this._service.Update(id, sub, x.ToRecord()))
      .Then(x => (x?.ToResp()).ToResult());
    return this.ReturnNullableResult(user, new EntityNotFound(
      "User Not Found", typeof(UserPrincipal), id.ToString()));
  }

  [Authorize(Policy = AuthPolicies.OnlyAdmin), HttpDelete("{id:guid}")]
  public async Task<ActionResult> Delete(Guid id)
  {
    var user = await this._service.Delete(id);
    return this.ReturnUnitNullableResult(user, new EntityNotFound(
      "User Not Found", typeof(UserPrincipal), id.ToString()));
  }
}
