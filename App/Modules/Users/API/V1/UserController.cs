using System.Net.Mime;
using App.Error.V1;
using App.Modules.Common;
using App.StartUp.Registry;
using App.Utility;
using Asp.Versioning;
using CSharp_Result;
using Domain.Model;
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
  private readonly ITokenService _token;

  private readonly CreateUserReqValidator _createUserReqValidator;
  private readonly UpdateUserReqValidator _updateUserReqValidator;
  private readonly UserSearchQueryValidator _userSearchQueryValidator;


  public UserController(IUserService service,
    CreateUserReqValidator createUserReqValidator, UpdateUserReqValidator updateUserReqValidator,
    UserSearchQueryValidator userSearchQueryValidator, ITokenService token)
  {
    this._service = service;
    this._createUserReqValidator = createUserReqValidator;
    this._updateUserReqValidator = updateUserReqValidator;
    this._userSearchQueryValidator = userSearchQueryValidator;
    this._token = token;
  }

  [Authorize(Policy = AuthPolicies.OnlyAdmin), HttpGet]
  public async Task<ActionResult<IEnumerable<UserPrincipalResp>>> Search([FromQuery] SearchUserQuery query)
  {
    var x = await this._userSearchQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchUserQuery")
      .ThenAwait(q => this._service.Search(q.ToDomain()))
      .Then(x => x.Select(u => u.ToResp()).ToResult());
    return this.ReturnResult(x);
  }

  [Authorize, HttpGet("{id}")]
  public async Task<ActionResult<UserResp>> GetById(string id)
  {
    var user = await this._service.GetById(id)
      .Then(x => (x?.ToResp()).ToResult())
      .Then(x =>
      {
        if (x?.Principal?.Id == this.Sub()) return x.ToResult();
        return new Unauthorized("You are not authorized to access this resource")
          .ToException();
      });
    return this.ReturnNullableResult(user, new EntityNotFound(
      "User Not Found", typeof(User), id.ToString()));
  }


  [Authorize, HttpGet("username/{username}")]
  public async Task<ActionResult<UserResp>> GetByUsername(string username)
  {
    var user = await this._service.GetByUsername(username)
      .Then(x => (x?.ToResp()).ToResult())
      .Then(x =>
      {
        if (x?.Principal?.Id == this.Sub()) return x.ToResult();
        return new Unauthorized("You are not authorized to access this resource")
          .ToException();
      });
    return this.ReturnNullableResult(user, new EntityNotFound(
      "User Not Found", typeof(User), username));
  }

  [Authorize, HttpGet("exist/{username}")]
  public async Task<ActionResult<bool>> Exist(string username)
  {
    var exist = await this._service.Exists(username);
    return this.ReturnResult(exist);
  }

  [Authorize, HttpPost]
  public async Task<ActionResult<UserPrincipalResp>> Create([FromBody] CreateUserReq req)
  {
    var id = this.Sub();
    if (id == null)
    {
      Result<UserPrincipalResp> x = new Unauthorized("You are not authorized to access this resource").ToException();
      return this.ReturnResult(x);
    }

    var user = await this._createUserReqValidator
      .ValidateAsyncResult(req, "Invalid CreateUserReq")
      .ThenAwait(x => this._service.Create(id, x.ToRecord()))
      .Then(x => x.ToResp().ToResult());
    return this.ReturnResult(user);
  }

  [Authorize, HttpPut("{id}")]
  public async Task<ActionResult<UserPrincipalResp>> Update(string id, [FromBody] UpdateUserReq req)
  {
    var sub = this.Sub();
    if (sub == null || sub != id)
    {
      Result<UserPrincipalResp> x = new Unauthorized("You are not authorized to access this resource").ToException();
      return this.ReturnResult(x);
    }

    var user = await this._updateUserReqValidator
      .ValidateAsyncResult(req, "Invalid UpdateUserReq")
      .ThenAwait(x => this._service.Update(id, x.ToRecord()))
      .Then(x => (x?.ToResp()).ToResult());
    return this.ReturnNullableResult(user, new EntityNotFound(
      "User Not Found", typeof(UserPrincipal), id.ToString()));
  }

  [Authorize(Policy = AuthPolicies.OnlyAdmin), HttpDelete("{id:guid}")]
  public async Task<ActionResult> Delete(string id)
  {
    var user = await this._service.Delete(id);
    return this.ReturnUnitNullableResult(user, new EntityNotFound(
      "User Not Found", typeof(UserPrincipal), id.ToString()));
  }

  [Authorize, HttpGet("{userId}/tokens")]
  public async Task<ActionResult<IEnumerable<TokenPrincipalResp>>> GetTokens(string userId)
  {
    var sub = this.Sub();
    if (sub == null || sub != userId)
    {
      Result<IEnumerable<TokenPrincipalResp>> x = new Unauthorized("You are not authorized to access this resource")
        .ToException();
      return this.ReturnResult(x);
    }

    var tokens = await this._token.Search(sub)
      .Then(x => x.Select(t => t.ToResp()), Errors.MapAll);
    return this.ReturnResult(tokens);
  }

  [Authorize, HttpPost("{userId}/tokens")]
  public async Task<ActionResult<TokenOTPrincipalResp>> CreateToken(string userId, [FromBody] CreateTokenReq req)
  {
    var sub = this.Sub();
    if (sub == null || sub != userId)
    {
      Result<TokenOTPrincipalResp> x = new Unauthorized("You are not authorized to access this resource").ToException();
      return this.ReturnResult(x);
    }

    var token = await this._token.Create(sub, req.ToRecord())
      .Then(x => x.ToOTResp(), Errors.MapAll);
    return this.ReturnResult(token);
  }

  [Authorize, HttpPut("{userId}/tokens/{tokenId:guid}")]
  public async Task<ActionResult<TokenPrincipalResp>> UpdateToken(string userId, Guid tokenId,
    [FromBody] CreateTokenReq req)
  {
    var sub = this.Sub();
    if (sub == null || sub != userId)
    {
      Result<TokenPrincipalResp> x = new Unauthorized("You are not authorized to access this resource").ToException();
      return this.ReturnResult(x);
    }

    var token = await this._token.Update(sub, tokenId, req.ToRecord())
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(token,
      new EntityNotFound("Cannot update entity that does not exist", typeof(TokenPrincipal), tokenId.ToString())
    );
  }

  [Authorize, HttpPost("{userId}/tokens/{tokenId:guid}/revoke")]
  public async Task<ActionResult> RevokeToken(string userId, Guid tokenId)
  {
    var sub = this.Sub();
    if (sub == null || sub != userId)
    {
      Result<Unit> x = new Unauthorized("You are not authorized to access this resource").ToException();
      return this.ReturnUnitResult(x);
    }

    var token = await this._token.Revoke(sub, tokenId);
    return this.ReturnUnitNullableResult(token,
      new EntityNotFound("Cannot revoke entity that does not exist", typeof(TokenPrincipal), tokenId.ToString()));
  }

  [Authorize, HttpDelete("{userId}/tokens/{tokenId:guid}")]
  public async Task<ActionResult> DeleteToken(string userId, Guid tokenId)
  {
    var sub = this.Sub();
    if (sub == null || sub != userId)
    {
      Result<Unit> x = new Unauthorized("You are not authorized to access this resource").ToException();
      return this.ReturnUnitResult(x);
    }

    var token = await this._token.Delete(sub, tokenId);
    return this.ReturnUnitNullableResult(token,
      new EntityNotFound("Cannot delete entity that does not exist", typeof(TokenPrincipal), tokenId.ToString()));
  }
}
