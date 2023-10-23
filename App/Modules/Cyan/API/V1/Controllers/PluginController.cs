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
public class PluginController : AtomiControllerBase
{
  private readonly IPluginService _service;
  private readonly IUserService _userService;
  private readonly CreatePluginReqValidator _createPluginReqValidator;
  private readonly UpdatePluginReqValidator _updatePluginReqValidator;
  private readonly SearchPluginQueryValidator _searchPluginQueryValidator;
  private readonly CreatePluginVersionReqValidator _createPluginVersionReqValidator;
  private readonly UpdatePluginVersionReqValidator _updatePluginVersionReqValidator;
  private readonly SearchPluginVersionQueryValidator _searchPluginVersionQueryValidator;


  public PluginController(IPluginService service,
    CreatePluginReqValidator createPluginReqValidator, UpdatePluginReqValidator updatePluginReqValidator,
    SearchPluginQueryValidator searchPluginQueryValidator,
    CreatePluginVersionReqValidator createPluginVersionReqValidator,
    UpdatePluginVersionReqValidator updatePluginVersionReqValidator,
    SearchPluginVersionQueryValidator searchPluginVersionQueryValidator, IUserService userService)
  {
    this._service = service;
    this._createPluginReqValidator = createPluginReqValidator;
    this._updatePluginReqValidator = updatePluginReqValidator;
    this._searchPluginQueryValidator = searchPluginQueryValidator;
    this._createPluginVersionReqValidator = createPluginVersionReqValidator;
    this._updatePluginVersionReqValidator = updatePluginVersionReqValidator;
    this._searchPluginVersionQueryValidator = searchPluginVersionQueryValidator;
    this._userService = userService;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<PluginPrincipalResp>>> Search([FromQuery] SearchPluginQuery query)
  {
    var plugins = await this._searchPluginQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchPluginQuery")
      .ThenAwait(x => this._service.Search(x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp())
        .ToResult());

    return this.ReturnResult(plugins);
  }


  [HttpGet("{id:guid}")]
  public async Task<ActionResult<PluginResp>> Get(Guid id)
  {
    var plugin = await this._service.Get(id)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(plugin,
      new EntityNotFound("Plugin not found", typeof(PluginPrincipal), id.ToString()));
  }

  [HttpGet("{username}/{name}")]
  public async Task<ActionResult<PluginResp>> Get(string username, string name)
  {
    var plugin = await this._service.Get(username, name)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(plugin,
      new EntityNotFound("Plugin not found", typeof(PluginPrincipal), $"{username}/{name}"));
  }

  [Authorize, HttpPost("{userId}")]
  public async Task<ActionResult<PluginPrincipalResp>> Create(string userId, [FromBody] CreatePluginReq req)
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<PluginPrincipalResp> e = new Unauthorized("You are not authorized to create a plugin for this user")
        .ToException();
      return this.ReturnResult(e);
    }

    var plugin = await this._createPluginReqValidator
      .ValidateAsyncResult(req, "Invalid CreatePluginReq")
      .ThenAwait(x => this._service.Create(userId, x.ToDomain().Item1, x.ToDomain().Item2))
      .Then(x => x.ToResp(), Errors.MapAll);
    return this.ReturnResult(plugin);
  }


  [Authorize, HttpPut("{userId}/{pluginId}")]
  public async Task<ActionResult<PluginPrincipalResp>> Update(string userId, Guid pluginId,
    [FromBody] UpdatePluginReq req)
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<PluginPrincipalResp> e = new Unauthorized("You are not authorized to create a plugin for this user")
        .ToException();
      return this.ReturnResult(e);
    }

    var plugin = await this._updatePluginReqValidator
      .ValidateAsyncResult(req, "Invalid UpdatePluginReq")
      .ThenAwait(x => this._service.Update(userId, pluginId, x.ToDomain()))
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(plugin, new EntityNotFound("Plugin not found", typeof(PluginPrincipal), pluginId.ToString()));
  }

  [Authorize, HttpPost("{username}/{pluginName}/like/{likerId}/{like}")]
  public async Task<ActionResult<Unit>> Like(string username, string pluginName, string likerId, bool like)
  {
    var sub = this.Sub();
    if (sub != likerId)
    {
      Result<Unit> e = new Unauthorized("You are not authorized to like this plugin")
        .ToException();
      return this.ReturnResult(e);
    }

    var plugin = await this._service.Like(likerId, username, pluginName, like)
      .Then(x => x.ToResult());
    return this.ReturnUnitNullableResult(plugin,
      new EntityNotFound("Plugin not found", typeof(PluginPrincipal), $"{username}/{pluginName}"));
  }

  [Authorize(Policy = AuthPolicies.OnlyAdmin), HttpDelete("{userId}/{pluginId:guid}")]
  public async Task<ActionResult<Unit>> Delete(string userId, Guid pluginId)
  {
    var plugin = await this._service.Delete(userId, pluginId)
      .Then(x => x.ToResult());
    return this.ReturnUnitNullableResult(plugin,
      new EntityNotFound("Plugin not found", typeof(PluginPrincipal), $"{userId}/{pluginId}"));
  }

  [HttpGet("{username}/{pluginName}/versions")]
  public async Task<ActionResult<IEnumerable<PluginVersionPrincipalResp>>> SearchVersion(string username,
    string pluginName, [FromQuery] SearchPluginVersionQuery query)
  {
    var plugins = await this._searchPluginVersionQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchPluginVersionQuery")
      .ThenAwait(x => this._service.SearchVersion(username, pluginName, x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp())
        .ToResult());
    return this.ReturnResult(plugins);
  }

  [HttpGet("{userId}/{pluginId:guid}/versions")]
  public async Task<ActionResult<IEnumerable<PluginVersionPrincipalResp>>> SearchVersion(string userId, Guid pluginId,
    [FromQuery] SearchPluginVersionQuery query)
  {
    var plugins = await this._searchPluginVersionQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchPluginVersionQuery")
      .ThenAwait(x => this._service.SearchVersion(userId, pluginId, x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp())
        .ToResult());
    return this.ReturnResult(plugins);
  }

  [HttpGet("{username}/{pluginName}/versions/{ver}")]
  public async Task<ActionResult<PluginVersionPrincipalResp>> GetVersion(string username, string pluginName, ulong ver,
    bool bumpDownload)
  {
    var plugin = await this._service.GetVersion(username, pluginName, ver, bumpDownload)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(plugin,
      new EntityNotFound("Plugin not found", typeof(PluginVersionPrincipal), $"{username}/{pluginName}:{ver}"));
  }

  [HttpGet("{userId}/{pluginId:guid}/versions/{ver}")]
  public async Task<ActionResult<PluginVersionPrincipalResp>> GetVersion(string userId, Guid pluginId, ulong ver)
  {
    var plugin = await this._service.GetVersion(userId, pluginId, ver)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(plugin,
      new EntityNotFound("Plugin not found", typeof(PluginVersionPrincipal), $"{userId}/{pluginId}:{ver}"));
  }

  [Authorize, HttpPost("{username}/{pluginName}/versions")]
  public async Task<ActionResult<PluginVersionPrincipalResp>> CreateVersion(string username, string pluginName,
    [FromBody] CreatePluginVersionReq req)
  {
    var sub = this.Sub();
    var version = await this._userService
      .GetByUsername(username)
      .ThenAwait(x => Task.FromResult(x?.Principal.Id == sub), Errors.MapAll)
      .ThenAwait(async x =>
      {
        if (x)
        {
          return await this._createPluginVersionReqValidator
            .ValidateAsyncResult(req, "Invalid CreatePluginVersionReq")
            .ThenAwait(c => this._service.CreateVersion(username, pluginName, c.ToDomain().Item2, c.ToDomain().Item1))
            .Then(c => c?.ToResp(), Errors.MapAll);
        }

        return new Unauthorized("You are not authorized to create a plugin for this user")
          .ToException();
      });
    return this.ReturnNullableResult(version,
      new EntityNotFound("Plugin not found", typeof(PluginPrincipal), $"{username}/{pluginName}"));
  }

  [Authorize, HttpPost("{userId}/{pluginId:guid}/versions")]
  public async Task<ActionResult<PluginVersionPrincipalResp>> CreateVersion(string userId, Guid pluginId,
    [FromBody] CreatePluginVersionReq req)
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<PluginVersionPrincipalResp> e = new Unauthorized("You are not authorized to create a plugin for this user")
        .ToException();
      return this.ReturnResult(e);
    }

    var version = await this._createPluginVersionReqValidator
      .ValidateAsyncResult(req, "Invalid CreatePluginVersionReq")
      .ThenAwait(x => this._service.CreateVersion(userId, pluginId, x.ToDomain().Item2, x.ToDomain().Item1))
      .Then(x => x?.ToResp(), Errors.MapAll);

    return this.ReturnNullableResult(version,
      new EntityNotFound("Plugin not found", typeof(PluginPrincipal), $"{userId}/{pluginId}"));
  }

  [Authorize, HttpPut("{userId}/{pluginId:guid}/versions/{ver}")]
  public async Task<ActionResult<PluginVersionPrincipalResp>> UpdateVersion(string userId, Guid pluginId, ulong ver,
    [FromBody] UpdatePluginVersionReq req)
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<PluginVersionPrincipalResp> e = new Unauthorized("You are not authorized to create a plugin for this user")
        .ToException();
      return this.ReturnResult(e);
    }

    var version = await this._updatePluginVersionReqValidator
      .ValidateAsyncResult(req, "Invalid UpdatePluginVersionReq")
      .ThenAwait(x => this._service.UpdateVersion(userId, pluginId, ver, x.ToDomain()))
      .Then(x => x?.ToResp(), Errors.MapAll);

    return this.ReturnNullableResult(version,
      new EntityNotFound("Plugin not found", typeof(PluginPrincipal), $"{userId}/{pluginId}"));
  }

}
