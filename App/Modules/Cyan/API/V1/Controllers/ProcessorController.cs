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
public class ProcessorController : AtomiControllerBase
{
  private readonly IProcessorService _service;
  private readonly IUserService _userService;
  private readonly CreateProcessorReqValidator _createProcessorReqValidator;
  private readonly UpdateProcessorReqValidator _updateProcessorReqValidator;
  private readonly SearchProcessorQueryValidator _searchProcessorQueryValidator;
  private readonly CreateProcessorVersionReqValidator _createProcessorVersionReqValidator;
  private readonly UpdateProcessorVersionReqValidator _updateProcessorVersionReqValidator;
  private readonly SearchProcessorVersionQueryValidator _searchProcessorVersionQueryValidator;


  public ProcessorController(IProcessorService service,
    CreateProcessorReqValidator createProcessorReqValidator, UpdateProcessorReqValidator updateProcessorReqValidator,
    SearchProcessorQueryValidator searchProcessorQueryValidator,
    CreateProcessorVersionReqValidator createProcessorVersionReqValidator,
    UpdateProcessorVersionReqValidator updateProcessorVersionReqValidator,
    SearchProcessorVersionQueryValidator searchProcessorVersionQueryValidator, IUserService userService)
  {
    this._service = service;
    this._createProcessorReqValidator = createProcessorReqValidator;
    this._updateProcessorReqValidator = updateProcessorReqValidator;
    this._searchProcessorQueryValidator = searchProcessorQueryValidator;
    this._createProcessorVersionReqValidator = createProcessorVersionReqValidator;
    this._updateProcessorVersionReqValidator = updateProcessorVersionReqValidator;
    this._searchProcessorVersionQueryValidator = searchProcessorVersionQueryValidator;
    this._userService = userService;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<ProcessorPrincipalResp>>> Search([FromQuery] SearchProcessorQuery query)
  {
    var processors = await this._searchProcessorQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchProcessorQuery")
      .ThenAwait(x => this._service.Search(x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp())
        .ToResult());

    return this.ReturnResult(processors);
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<ProcessorResp>> Get(Guid id)
  {
    var processor = await this._service.Get(id)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(processor,
      new EntityNotFound("Processor not found", typeof(ProcessorPrincipal), id.ToString()));
  }

  [HttpGet("{username}/{name}")]
  public async Task<ActionResult<ProcessorResp>> Get(string username, string name)
  {
    var processor = await this._service.Get(username, name)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(processor,
      new EntityNotFound("Processor not found", typeof(ProcessorPrincipal), $"{username}/{name}"));
  }

  [Authorize, HttpPost("{userId}")]
  public async Task<ActionResult<ProcessorPrincipalResp>> Create(string userId, [FromBody] CreateProcessorReq req)
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<ProcessorPrincipalResp> e = new Unauthorized("You are not authorized to create a processor for this user")
        .ToException();
      return this.ReturnResult(e);
    }

    var processor = await this._createProcessorReqValidator
      .ValidateAsyncResult(req, "Invalid CreateProcessorReq")
      .ThenAwait(x => this._service.Create(userId, x.ToDomain().Item1, x.ToDomain().Item2))
      .Then(x => x.ToResp(), Errors.MapAll);
    return this.ReturnResult(processor);
  }


  [Authorize, HttpPut("{userId}/{processorId}")]
  public async Task<ActionResult<ProcessorPrincipalResp>> Update(string userId, Guid processorId,
    [FromBody] UpdateProcessorReq req)
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<ProcessorPrincipalResp> e = new Unauthorized("You are not authorized to create a processor for this user")
        .ToException();
      return this.ReturnResult(e);
    }

    var processor = await this._updateProcessorReqValidator
      .ValidateAsyncResult(req, "Invalid UpdateProcessorReq")
      .ThenAwait(x => this._service.Update(userId, processorId, x.ToDomain()))
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(processor, new EntityNotFound("Processor not found", typeof(ProcessorPrincipal), processorId.ToString()));
  }

  [Authorize, HttpPost("{username}/{processorName}/like/{likerId}/{like}")]
  public async Task<ActionResult<Unit>> Like(string username, string processorName, string likerId, bool like)
  {
    var sub = this.Sub();
    if (sub != likerId)
    {
      Result<Unit> e = new Unauthorized("You are not authorized to like this processor")
        .ToException();
      return this.ReturnResult(e);
    }

    var processor = await this._service.Like(likerId, username, processorName, like)
      .Then(x => x.ToResult());
    return this.ReturnUnitNullableResult(processor,
      new EntityNotFound("Processor not found", typeof(ProcessorPrincipal), $"{username}/{processorName}"));
  }

  [Authorize(Policy = AuthPolicies.OnlyAdmin), HttpDelete("{userId}/{processorId:guid}")]
  public async Task<ActionResult<Unit>> Delete(string userId, Guid processorId)
  {
    var processor = await this._service.Delete(userId, processorId)
      .Then(x => x.ToResult());
    return this.ReturnUnitNullableResult(processor,
      new EntityNotFound("Processor not found", typeof(ProcessorPrincipal), $"{userId}/{processorId}"));
  }

  [HttpGet("{username}/{processorName}/versions")]
  public async Task<ActionResult<IEnumerable<ProcessorVersionPrincipalResp>>> SearchVersion(string username,
    string processorName, [FromQuery] SearchProcessorVersionQuery query)
  {
    var processors = await this._searchProcessorVersionQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchProcessorVersionQuery")
      .ThenAwait(x => this._service.SearchVersion(username, processorName, x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp())
        .ToResult());
    return this.ReturnResult(processors);
  }

  [HttpGet("{userId}/{processorId:guid}/versions")]
  public async Task<ActionResult<IEnumerable<ProcessorVersionPrincipalResp>>> SearchVersion(string userId, Guid processorId,
    [FromQuery] SearchProcessorVersionQuery query)
  {
    var processors = await this._searchProcessorVersionQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchProcessorVersionQuery")
      .ThenAwait(x => this._service.SearchVersion(userId, processorId, x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp())
        .ToResult());
    return this.ReturnResult(processors);
  }

  [HttpGet("{username}/{processorName}/versions/{ver}")]
  public async Task<ActionResult<ProcessorVersionPrincipalResp>> GetVersion(string username, string processorName, ulong ver,
    bool bumpDownload)
  {
    var processor = await this._service.GetVersion(username, processorName, ver, bumpDownload)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(processor,
      new EntityNotFound("Processor not found", typeof(ProcessorVersionPrincipal), $"{username}/{processorName}:{ver}"));
  }

  [HttpGet("{userId}/{processorId:guid}/versions/{ver}")]
  public async Task<ActionResult<ProcessorVersionPrincipalResp>> GetVersion(string userId, Guid processorId, ulong ver)
  {
    var processor = await this._service.GetVersion(userId, processorId, ver)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(processor,
      new EntityNotFound("Processor not found", typeof(ProcessorVersionPrincipal), $"{userId}/{processorId}:{ver}"));
  }

  [Authorize, HttpPost("{username}/{processorName}/versions")]
  public async Task<ActionResult<ProcessorVersionPrincipalResp>> CreateVersion(string username, string processorName,
    [FromBody] CreateProcessorVersionReq req)
  {
    var sub = this.Sub();
    var version = await this._userService
      .GetByUsername(username)
      .ThenAwait(x => Task.FromResult(x?.Principal.Id == sub), Errors.MapAll)
      .ThenAwait(async x =>
      {
        if (x)
        {
          return await this._createProcessorVersionReqValidator
            .ValidateAsyncResult(req, "Invalid CreateProcessorVersionReq")
            .ThenAwait(c => this._service.CreateVersion(username, processorName, c.ToDomain().Item2, c.ToDomain().Item1))
            .Then(c => c?.ToResp(), Errors.MapAll);
        }

        return new Unauthorized("You are not authorized to create a processor for this user")
          .ToException();
      });
    return this.ReturnNullableResult(version,
      new EntityNotFound("Processor not found", typeof(ProcessorPrincipal), $"{username}/{processorName}"));
  }

  [Authorize, HttpPost("{userId}/{processorId:guid}/versions")]
  public async Task<ActionResult<ProcessorVersionPrincipalResp>> CreateVersion(string userId, Guid processorId,
    [FromBody] CreateProcessorVersionReq req)
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<ProcessorVersionPrincipalResp> e = new Unauthorized("You are not authorized to create a processor for this user")
        .ToException();
      return this.ReturnResult(e);
    }

    var version = await this._createProcessorVersionReqValidator
      .ValidateAsyncResult(req, "Invalid CreateProcessorVersionReq")
      .ThenAwait(x => this._service.CreateVersion(userId, processorId, x.ToDomain().Item2, x.ToDomain().Item1))
      .Then(x => x?.ToResp(), Errors.MapAll);

    return this.ReturnNullableResult(version,
      new EntityNotFound("Processor not found", typeof(ProcessorPrincipal), $"{userId}/{processorId}"));
  }

  [Authorize, HttpPut("{userId}/{processorId:guid}/versions/{ver}")]
  public async Task<ActionResult<ProcessorVersionPrincipalResp>> UpdateVersion(string userId, Guid processorId, ulong ver,
    [FromBody] UpdateProcessorVersionReq req)
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<ProcessorVersionPrincipalResp> e = new Unauthorized("You are not authorized to create a processor for this user")
        .ToException();
      return this.ReturnResult(e);
    }

    var version = await this._updateProcessorVersionReqValidator
      .ValidateAsyncResult(req, "Invalid UpdateProcessorVersionReq")
      .ThenAwait(x => this._service.UpdateVersion(userId, processorId, ver, x.ToDomain()))
      .Then(x => x?.ToResp(), Errors.MapAll);

    return this.ReturnNullableResult(version,
      new EntityNotFound("Processor not found", typeof(ProcessorPrincipal), $"{userId}/{processorId}"));
  }

}
