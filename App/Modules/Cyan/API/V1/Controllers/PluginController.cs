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
public class PluginController(
  IPluginService service,
  CreatePluginReqValidator createPluginReqValidator,
  UpdatePluginReqValidator updatePluginReqValidator,
  SearchPluginQueryValidator searchPluginQueryValidator,
  CreatePluginVersionReqValidator createPluginVersionReqValidator,
  UpdatePluginVersionReqValidator updatePluginVersionReqValidator,
  SearchPluginVersionQueryValidator searchPluginVersionQueryValidator,
  IUserService userService,
  PushPluginReqValidator pluginReqValidator
) : AtomiControllerBase
{
  [HttpGet]
  public async Task<ActionResult<IEnumerable<PluginPrincipalResp>>> Search(
    [FromQuery] SearchPluginQuery query
  )
  {
    var plugins = await searchPluginQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchPluginQuery")
      .ThenAwait(x => service.Search(x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp()).ToResult());

    return this.ReturnResult(plugins);
  }

  [HttpGet("id/{userId}/{pluginId:guid}")]
  public async Task<ActionResult<PluginResp>> Get(string userId, Guid pluginId)
  {
    var plugin = await service.Get(userId, pluginId).Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      plugin,
      new EntityNotFound("Plugin not found", typeof(PluginPrincipal), pluginId.ToString())
    );
  }

  [HttpGet("slug/{username}/{name}")]
  public async Task<ActionResult<PluginResp>> Get(string username, string name)
  {
    var plugin = await service.Get(username, name).Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      plugin,
      new EntityNotFound("Plugin not found", typeof(PluginPrincipal), $"{username}/{name}")
    );
  }

  [Authorize, HttpPost("id/{userId}")]
  public async Task<ActionResult<PluginPrincipalResp>> Create(
    string userId,
    [FromBody] CreatePluginReq req
  )
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<PluginPrincipalResp> e = new Unauthorized(
        "You are not authorized to create a plugin for this user"
      ).ToException();
      return this.ReturnResult(e);
    }

    var plugin = await createPluginReqValidator
      .ValidateAsyncResult(req, "Invalid CreatePluginReq")
      .ThenAwait(x => service.Create(userId, x.ToDomain().Item1, x.ToDomain().Item2))
      .Then(x => x.ToResp(), Errors.MapAll);
    return this.ReturnResult(plugin);
  }

  [Authorize, HttpPut("id/{userId}/{pluginId}")]
  public async Task<ActionResult<PluginPrincipalResp>> Update(
    string userId,
    Guid pluginId,
    [FromBody] UpdatePluginReq req
  )
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<PluginPrincipalResp> e = new Unauthorized(
        "You are not authorized to create a plugin for this user"
      ).ToException();
      return this.ReturnResult(e);
    }

    var plugin = await updatePluginReqValidator
      .ValidateAsyncResult(req, "Invalid UpdatePluginReq")
      .ThenAwait(x => service.Update(userId, pluginId, x.ToDomain()))
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      plugin,
      new EntityNotFound("Plugin not found", typeof(PluginPrincipal), pluginId.ToString())
    );
  }

  [Authorize, HttpPost("slug/{username}/{pluginName}/like/{likerId}/{like}")]
  public async Task<ActionResult<Unit>> Like(
    string username,
    string pluginName,
    string likerId,
    bool like
  )
  {
    var sub = this.Sub();
    if (sub != likerId)
    {
      Result<Unit> e = new Unauthorized("You are not authorized to like this plugin").ToException();
      return this.ReturnResult(e);
    }

    var plugin = await service.Like(likerId, username, pluginName, like).Then(x => x.ToResult());
    return this.ReturnUnitNullableResult(
      plugin,
      new EntityNotFound("Plugin not found", typeof(PluginPrincipal), $"{username}/{pluginName}")
    );
  }

  [Authorize(Policy = AuthPolicies.OnlyAdmin), HttpDelete("id/{userId}/{pluginId:guid}")]
  public async Task<ActionResult<Unit>> Delete(string userId, Guid pluginId)
  {
    var plugin = await service.Delete(userId, pluginId).Then(x => x.ToResult());
    return this.ReturnUnitNullableResult(
      plugin,
      new EntityNotFound("Plugin not found", typeof(PluginPrincipal), $"{userId}/{pluginId}")
    );
  }

  [HttpGet("slug/{username}/{pluginName}/versions")]
  public async Task<ActionResult<IEnumerable<PluginVersionPrincipalResp>>> SearchVersion(
    string username,
    string pluginName,
    [FromQuery] SearchPluginVersionQuery query
  )
  {
    var plugins = await searchPluginVersionQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchPluginVersionQuery")
      .ThenAwait(x => service.SearchVersion(username, pluginName, x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp()).ToResult());
    return this.ReturnResult(plugins);
  }

  [HttpGet("id/{userId}/{pluginId:guid}/versions")]
  public async Task<ActionResult<IEnumerable<PluginVersionPrincipalResp>>> SearchVersion(
    string userId,
    Guid pluginId,
    [FromQuery] SearchPluginVersionQuery query
  )
  {
    var plugins = await searchPluginVersionQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchPluginVersionQuery")
      .ThenAwait(x => service.SearchVersion(userId, pluginId, x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp()).ToResult());
    return this.ReturnResult(plugins);
  }

  [HttpGet("slug/{username}/{pluginName}/versions/{ver}")]
  public async Task<ActionResult<PluginVersionResp>> GetVersion(
    string username,
    string pluginName,
    ulong ver,
    bool bumpDownload
  )
  {
    var plugin = await service
      .GetVersion(username, pluginName, ver, bumpDownload)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      plugin,
      new EntityNotFound(
        "Plugin not found",
        typeof(PluginVersion),
        $"{username}/{pluginName}:{ver}"
      )
    );
  }

  [HttpGet("slug/{username}/{pluginName}/versions/latest")]
  public async Task<ActionResult<PluginVersionResp>> GetVersion(
    string username,
    string pluginName,
    bool bumpDownload
  )
  {
    var plugin = await service
      .GetVersion(username, pluginName, bumpDownload)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      plugin,
      new EntityNotFound("Plugin not found", typeof(PluginVersion), $"{username}/{pluginName}")
    );
  }

  [HttpGet("id/{userId}/{pluginId:guid}/versions/{ver}")]
  public async Task<ActionResult<PluginVersionResp>> GetVersion(
    string userId,
    Guid pluginId,
    ulong ver
  )
  {
    var plugin = await service
      .GetVersion(userId, pluginId, ver)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      plugin,
      new EntityNotFound("Plugin not found", typeof(PluginVersion), $"{userId}/{pluginId}:{ver}")
    );
  }

  [Authorize, HttpPost("slug/{username}/{pluginName}/versions")]
  public async Task<ActionResult<PluginVersionPrincipalResp>> CreateVersion(
    string username,
    string pluginName,
    [FromBody] CreatePluginVersionReq req
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
          return await createPluginVersionReqValidator
            .ValidateAsyncResult(req, "Invalid CreatePluginVersionReq")
            .ThenAwait(c =>
              service.CreateVersion(username, pluginName, c.ToDomain().Item2, c.ToDomain().Item1)
            )
            .Then(c => c?.ToResp(), Errors.MapAll);
        }

        return new Unauthorized(
          "You are not authorized to create a plugin for this user"
        ).ToException();
      });
    return this.ReturnNullableResult(
      version,
      new EntityNotFound("Plugin not found", typeof(PluginPrincipal), $"{username}/{pluginName}")
    );
  }

  [Authorize, HttpPost("id/{userId}/{pluginId:guid}/versions")]
  public async Task<ActionResult<PluginVersionPrincipalResp>> CreateVersion(
    string userId,
    Guid pluginId,
    [FromBody] CreatePluginVersionReq req
  )
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<PluginVersionPrincipalResp> e = new Unauthorized(
        "You are not authorized to create a plugin for this user"
      ).ToException();
      return this.ReturnResult(e);
    }

    var version = await createPluginVersionReqValidator
      .ValidateAsyncResult(req, "Invalid CreatePluginVersionReq")
      .ThenAwait(x =>
        service.CreateVersion(userId, pluginId, x.ToDomain().Item2, x.ToDomain().Item1)
      )
      .Then(x => x?.ToResp(), Errors.MapAll);

    return this.ReturnNullableResult(
      version,
      new EntityNotFound("Plugin not found", typeof(PluginPrincipal), $"{userId}/{pluginId}")
    );
  }

  [Authorize, HttpPut("id/{userId}/{pluginId:guid}/versions/{ver}")]
  public async Task<ActionResult<PluginVersionPrincipalResp>> UpdateVersion(
    string userId,
    Guid pluginId,
    ulong ver,
    [FromBody] UpdatePluginVersionReq req
  )
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<PluginVersionPrincipalResp> e = new Unauthorized(
        "You are not authorized to create a plugin for this user"
      ).ToException();
      return this.ReturnResult(e);
    }

    var version = await updatePluginVersionReqValidator
      .ValidateAsyncResult(req, "Invalid UpdatePluginVersionReq")
      .ThenAwait(x => service.UpdateVersion(userId, pluginId, ver, x.ToDomain()))
      .Then(x => x?.ToResp(), Errors.MapAll);

    return this.ReturnNullableResult(
      version,
      new EntityNotFound("Plugin not found", typeof(PluginPrincipal), $"{userId}/{pluginId}")
    );
  }

  [Authorize, HttpPost("push/{username}")]
  public async Task<ActionResult<PluginVersionPrincipalResp>> CreateVersion(
    string username,
    [FromBody] PushPluginReq req
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
          return await pluginReqValidator
            .ValidateAsyncResult(req, "Invalid PushPluginReq")
            .Then(push => push.ToDomain(), Errors.MapAll)
            .ThenAwait(domain =>
            {
              var (record, metadata, vRecord, vProperty) = domain;
              return service.Push(username, record, metadata, vRecord, vProperty);
            })
            .Then(c => c?.ToResp(), Errors.MapAll);
        }

        return new Unauthorized(
          "You are not authorized to create a plugin for this user"
        ).ToException();
      });

    return this.ReturnNullableResult(
      version,
      new EntityNotFound("Plugin not found", typeof(PluginPrincipal), $"{username}/{req.Name}")
    );
  }

  [HttpGet("versions/{versionId:guid}")]
  public async Task<ActionResult<PluginVersionResp>> GetVersionById(Guid versionId)
  {
    var plugin = await service.GetVersionById(versionId).Then(x => x?.ToResp(), Errors.MapNone);
    return this.ReturnNullableResult(
      plugin,
      new EntityNotFound("Plugin version not found", typeof(PluginVersion), versionId.ToString())
    );
  }
}
