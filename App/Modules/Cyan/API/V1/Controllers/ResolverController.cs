using System.Net.Mime;
using App.Error.V1;
using App.Modules.Common;
using App.Modules.Cyan.API.V1.Mappers;
using App.Modules.Cyan.API.V1.Models;
using App.Modules.Cyan.API.V1.Validators;
using App.StartUp.Registry;
using App.Utility;
using Asp.Versioning;
using CSharp_Result;
using Domain.Model;
using Domain.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Modules.Cyan.API.V1.Controllers;

/// <summary>
/// V1 controller
/// </summary>
[ApiVersion(1.0)]
[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Route("api/v{version:apiVersion}/[controller]")]
public class ResolverController(
  IResolverService service,
  CreateResolverReqValidator createResolverReqValidator,
  UpdateResolverReqValidator updateResolverReqValidator,
  SearchResolverQueryValidator searchResolverQueryValidator,
  CreateResolverVersionReqValidator createResolverVersionReqValidator,
  UpdateResolverVersionReqValidator updateResolverVersionReqValidator,
  SearchResolverVersionQueryValidator searchResolverVersionQueryValidator,
  IUserService userService,
  PushResolverReqValidator resolverReqValidator
) : AtomiControllerBase
{
  [HttpGet]
  public async Task<ActionResult<IEnumerable<ResolverPrincipalResp>>> Search(
    [FromQuery] SearchResolverQuery query
  )
  {
    var resolvers = await searchResolverQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchResolverQuery")
      .ThenAwait(x => service.Search(x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp()).ToResult());

    return this.ReturnResult(resolvers);
  }

  [HttpGet("id/{userId}/{resolverId:guid}")]
  public async Task<ActionResult<ResolverResp>> Get(string userId, Guid resolverId)
  {
    var resolver = await service.Get(userId, resolverId).Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      resolver,
      new EntityNotFound("Resolver not found", typeof(ResolverPrincipal), resolverId.ToString())
    );
  }

  [HttpGet("slug/{username}/{name}")]
  public async Task<ActionResult<ResolverResp>> Get(string username, string name)
  {
    var resolver = await service.Get(username, name).Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      resolver,
      new EntityNotFound("Resolver not found", typeof(ResolverPrincipal), $"{username}/{name}")
    );
  }

  [Authorize, HttpPost("id/{userId}")]
  public async Task<ActionResult<ResolverPrincipalResp>> Create(
    string userId,
    [FromBody] CreateResolverReq req
  )
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<ResolverPrincipalResp> e = new Unauthorized(
        "You are not authorized to create a resolver for this user"
      ).ToException();
      return this.ReturnResult(e);
    }

    var resolver = await createResolverReqValidator
      .ValidateAsyncResult(req, "Invalid CreateResolverReq")
      .ThenAwait(x => service.Create(userId, x.ToDomain().Item1, x.ToDomain().Item2))
      .Then(x => x.ToResp(), Errors.MapAll);
    return this.ReturnResult(resolver);
  }

  [Authorize, HttpPut("id/{userId}/{resolverId}")]
  public async Task<ActionResult<ResolverPrincipalResp>> Update(
    string userId,
    Guid resolverId,
    [FromBody] UpdateResolverReq req
  )
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<ResolverPrincipalResp> e = new Unauthorized(
        "You are not authorized to create a resolver for this user"
      ).ToException();
      return this.ReturnResult(e);
    }

    var resolver = await updateResolverReqValidator
      .ValidateAsyncResult(req, "Invalid UpdateResolverReq")
      .ThenAwait(x => service.Update(userId, resolverId, x.ToDomain()))
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      resolver,
      new EntityNotFound("Resolver not found", typeof(ResolverPrincipal), resolverId.ToString())
    );
  }

  [Authorize, HttpPost("slug/{username}/{resolverName}/like/{likerId}/{like}")]
  public async Task<ActionResult<Unit>> Like(
    string username,
    string resolverName,
    string likerId,
    bool like
  )
  {
    var sub = this.Sub();
    if (sub != likerId)
    {
      Result<Unit> e = new Unauthorized(
        "You are not authorized to like this resolver"
      ).ToException();
      return this.ReturnResult(e);
    }

    var resolver = await service
      .Like(likerId, username, resolverName, like)
      .Then(x => x.ToResult());
    return this.ReturnUnitNullableResult(
      resolver,
      new EntityNotFound(
        "Resolver not found",
        typeof(ResolverPrincipal),
        $"{username}/{resolverName}"
      )
    );
  }

  [Authorize(Policy = AuthPolicies.OnlyAdmin), HttpDelete("id/{userId}/{resolverId:guid}")]
  public async Task<ActionResult<Unit>> Delete(string userId, Guid resolverId)
  {
    var resolver = await service.Delete(userId, resolverId).Then(x => x.ToResult());
    return this.ReturnUnitNullableResult(
      resolver,
      new EntityNotFound("Resolver not found", typeof(ResolverPrincipal), $"{userId}/{resolverId}")
    );
  }

  [HttpGet("slug/{username}/{resolverName}/versions")]
  public async Task<ActionResult<IEnumerable<ResolverVersionPrincipalResp>>> SearchVersion(
    string username,
    string resolverName,
    [FromQuery] SearchResolverVersionQuery query
  )
  {
    var resolvers = await searchResolverVersionQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchResolverVersionQuery")
      .ThenAwait(x => service.SearchVersion(username, resolverName, x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp()).ToResult());
    return this.ReturnResult(resolvers);
  }

  [HttpGet("id/{userId}/{resolverId:guid}/versions")]
  public async Task<ActionResult<IEnumerable<ResolverVersionPrincipalResp>>> SearchVersion(
    string userId,
    Guid resolverId,
    [FromQuery] SearchResolverVersionQuery query
  )
  {
    var resolvers = await searchResolverVersionQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchResolverVersionQuery")
      .ThenAwait(x => service.SearchVersion(userId, resolverId, x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp()).ToResult());
    return this.ReturnResult(resolvers);
  }

  [HttpGet("slug/{username}/{resolverName}/versions/{ver}")]
  public async Task<ActionResult<ResolverVersionResp>> GetVersion(
    string username,
    string resolverName,
    ulong ver,
    bool bumpDownload
  )
  {
    var resolver = await service
      .GetVersion(username, resolverName, ver, bumpDownload)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      resolver,
      new EntityNotFound(
        "Resolver not found",
        typeof(ResolverVersion),
        $"{username}/{resolverName}:{ver}"
      )
    );
  }

  [HttpGet("slug/{username}/{resolverName}/versions/latest")]
  public async Task<ActionResult<ResolverVersionResp>> GetVersion(
    string username,
    string resolverName,
    bool bumpDownload
  )
  {
    var resolver = await service
      .GetVersion(username, resolverName, bumpDownload)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      resolver,
      new EntityNotFound(
        "Resolver not found",
        typeof(ResolverVersion),
        $"{username}/{resolverName}"
      )
    );
  }

  [HttpGet("id/{userId}/{resolverId:guid}/versions/{ver}")]
  public async Task<ActionResult<ResolverVersionResp>> GetVersion(
    string userId,
    Guid resolverId,
    ulong ver
  )
  {
    var resolver = await service
      .GetVersion(userId, resolverId, ver)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      resolver,
      new EntityNotFound(
        "Resolver not found",
        typeof(ResolverVersion),
        $"{userId}/{resolverId}:{ver}"
      )
    );
  }

  [Authorize, HttpPost("slug/{username}/{resolverName}/versions")]
  public async Task<ActionResult<ResolverVersionPrincipalResp>> CreateVersion(
    string username,
    string resolverName,
    [FromBody] CreateResolverVersionReq req
  )
  {
    var sub = this.Sub();
    var version = await userService
      .GetByUsername(username)
      .ThenAwait(x => Task.FromResult(x?.Principal.Id == sub), Errors.MapAll)
      .ThenAwait(async x =>
      {
        if (x)
        {
          return await createResolverVersionReqValidator
            .ValidateAsyncResult(req, "Invalid CreateResolverVersionReq")
            .ThenAwait(c =>
              service.CreateVersion(username, resolverName, c.ToDomain().Item2, c.ToDomain().Item1)
            )
            .Then(c => c?.ToResp(), Errors.MapAll);
        }

        return new Unauthorized(
          "You are not authorized to create a resolver for this user"
        ).ToException();
      });
    return this.ReturnNullableResult(
      version,
      new EntityNotFound(
        "Resolver not found",
        typeof(ResolverPrincipal),
        $"{username}/{resolverName}"
      )
    );
  }

  [Authorize, HttpPost("id/{userId}/{resolverId:guid}/versions")]
  public async Task<ActionResult<ResolverVersionPrincipalResp>> CreateVersion(
    string userId,
    Guid resolverId,
    [FromBody] CreateResolverVersionReq req
  )
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<ResolverVersionPrincipalResp> e = new Unauthorized(
        "You are not authorized to create a resolver for this user"
      ).ToException();
      return this.ReturnResult(e);
    }

    var version = await createResolverVersionReqValidator
      .ValidateAsyncResult(req, "Invalid CreateResolverVersionReq")
      .ThenAwait(x =>
        service.CreateVersion(userId, resolverId, x.ToDomain().Item2, x.ToDomain().Item1)
      )
      .Then(x => x?.ToResp(), Errors.MapAll);

    return this.ReturnNullableResult(
      version,
      new EntityNotFound("Resolver not found", typeof(ResolverPrincipal), $"{userId}/{resolverId}")
    );
  }

  [Authorize, HttpPut("id/{userId}/{resolverId:guid}/versions/{ver}")]
  public async Task<ActionResult<ResolverVersionPrincipalResp>> UpdateVersion(
    string userId,
    Guid resolverId,
    ulong ver,
    [FromBody] UpdateResolverVersionReq req
  )
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<ResolverVersionPrincipalResp> e = new Unauthorized(
        "You are not authorized to create a resolver for this user"
      ).ToException();
      return this.ReturnResult(e);
    }

    var version = await updateResolverVersionReqValidator
      .ValidateAsyncResult(req, "Invalid UpdateResolverVersionReq")
      .ThenAwait(x => service.UpdateVersion(userId, resolverId, ver, x.ToDomain()))
      .Then(x => x?.ToResp(), Errors.MapAll);

    return this.ReturnNullableResult(
      version,
      new EntityNotFound("Resolver not found", typeof(ResolverPrincipal), $"{userId}/{resolverId}")
    );
  }

  [Authorize, HttpPost("push/{username}")]
  public async Task<ActionResult<ResolverVersionPrincipalResp>> CreateVersion(
    string username,
    [FromBody] PushResolverReq req
  )
  {
    var sub = this.Sub();
    var version = await userService
      .GetByUsername(username)
      .ThenAwait(x => Task.FromResult(x?.Principal.Id == sub), Errors.MapAll)
      .ThenAwait(async x =>
      {
        if (x)
        {
          return await resolverReqValidator
            .ValidateAsyncResult(req, "Invalid PushResolverReq")
            .Then(push => push.ToDomain(), Errors.MapAll)
            .ThenAwait(domain =>
            {
              var (record, metadata, vRecord, vProperty) = domain;
              return service.Push(username, record, metadata, vRecord, vProperty);
            })
            .Then(c => c?.ToResp(), Errors.MapAll);
        }

        return new Unauthorized(
          "You are not authorized to create a resolver for this user"
        ).ToException();
      });

    return this.ReturnNullableResult(
      version,
      new EntityNotFound("Resolver not found", typeof(ResolverPrincipal), $"{username}/{req.Name}")
    );
  }

  [HttpGet("versions/{versionId:guid}")]
  public async Task<ActionResult<ResolverVersionResp>> GetVersionById(Guid versionId)
  {
    var resolver = await service.GetVersionById(versionId).Then(x => x?.ToResp(), Errors.MapNone);
    return this.ReturnNullableResult(
      resolver,
      new EntityNotFound(
        "Resolver version not found",
        typeof(ResolverVersion),
        versionId.ToString()
      )
    );
  }
}
