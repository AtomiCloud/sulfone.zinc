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
public class ProcessorController(
  IProcessorService service,
  CreateProcessorReqValidator createProcessorReqValidator,
  UpdateProcessorReqValidator updateProcessorReqValidator,
  SearchProcessorQueryValidator searchProcessorQueryValidator,
  CreateProcessorVersionReqValidator createProcessorVersionReqValidator,
  UpdateProcessorVersionReqValidator updateProcessorVersionReqValidator,
  SearchProcessorVersionQueryValidator searchProcessorVersionQueryValidator,
  IUserService userService,
  PushProcessorReqValidator processorReqValidator)
  : AtomiControllerBase
{
  [HttpGet]
  public async Task<ActionResult<IEnumerable<ProcessorPrincipalResp>>> Search([FromQuery] SearchProcessorQuery query)
  {
    var processors = await searchProcessorQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchProcessorQuery")
      .ThenAwait(x => service.Search(x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp())
        .ToResult());

    return this.ReturnResult(processors);
  }

  [HttpGet("id/{userId}/{processorId:guid}")]
  public async Task<ActionResult<ProcessorResp>> Get(string userId, Guid processorId)
  {
    var processor = await service.Get(userId, processorId)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(processor,
      new EntityNotFound("Processor not found", typeof(ProcessorPrincipal), processorId.ToString()));
  }

  [HttpGet("slug/{username}/{name}")]
  public async Task<ActionResult<ProcessorResp>> Get(string username, string name)
  {
    var processor = await service.Get(username, name)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(processor,
      new EntityNotFound("Processor not found", typeof(ProcessorPrincipal), $"{username}/{name}"));
  }

  [Authorize, HttpPost("id/{userId}")]
  public async Task<ActionResult<ProcessorPrincipalResp>> Create(string userId, [FromBody] CreateProcessorReq req)
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<ProcessorPrincipalResp> e = new Unauthorized("You are not authorized to create a processor for this user")
        .ToException();
      return this.ReturnResult(e);
    }

    var processor = await createProcessorReqValidator
      .ValidateAsyncResult(req, "Invalid CreateProcessorReq")
      .ThenAwait(x => service.Create(userId, x.ToDomain().Item1, x.ToDomain().Item2))
      .Then(x => x.ToResp(), Errors.MapAll);
    return this.ReturnResult(processor);
  }


  [Authorize, HttpPut("id/{userId}/{processorId}")]
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

    var processor = await updateProcessorReqValidator
      .ValidateAsyncResult(req, "Invalid UpdateProcessorReq")
      .ThenAwait(x => service.Update(userId, processorId, x.ToDomain()))
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(processor, new EntityNotFound("Processor not found", typeof(ProcessorPrincipal), processorId.ToString()));
  }

  [Authorize, HttpPost("slug/{username}/{processorName}/like/{likerId}/{like}")]
  public async Task<ActionResult<Unit>> Like(string username, string processorName, string likerId, bool like)
  {
    var sub = this.Sub();
    if (sub != likerId)
    {
      Result<Unit> e = new Unauthorized("You are not authorized to like this processor")
        .ToException();
      return this.ReturnResult(e);
    }

    var processor = await service.Like(likerId, username, processorName, like)
      .Then(x => x.ToResult());
    return this.ReturnUnitNullableResult(processor,
      new EntityNotFound("Processor not found", typeof(ProcessorPrincipal), $"{username}/{processorName}"));
  }

  [Authorize(Policy = AuthPolicies.OnlyAdmin), HttpDelete("id/{userId}/{processorId:guid}")]
  public async Task<ActionResult<Unit>> Delete(string userId, Guid processorId)
  {
    var processor = await service.Delete(userId, processorId)
      .Then(x => x.ToResult());
    return this.ReturnUnitNullableResult(processor,
      new EntityNotFound("Processor not found", typeof(ProcessorPrincipal), $"{userId}/{processorId}"));
  }

  [HttpGet("slug/{username}/{processorName}/versions")]
  public async Task<ActionResult<IEnumerable<ProcessorVersionPrincipalResp>>> SearchVersion(string username,
    string processorName, [FromQuery] SearchProcessorVersionQuery query)
  {
    var processors = await searchProcessorVersionQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchProcessorVersionQuery")
      .ThenAwait(x => service.SearchVersion(username, processorName, x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp())
        .ToResult());
    return this.ReturnResult(processors);
  }

  [HttpGet("id/{userId}/{processorId:guid}/versions")]
  public async Task<ActionResult<IEnumerable<ProcessorVersionPrincipalResp>>> SearchVersion(string userId, Guid processorId,
    [FromQuery] SearchProcessorVersionQuery query)
  {
    var processors = await searchProcessorVersionQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchProcessorVersionQuery")
      .ThenAwait(x => service.SearchVersion(userId, processorId, x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp())
        .ToResult());
    return this.ReturnResult(processors);
  }

  [HttpGet("slug/{username}/{processorName}/versions/{ver}")]
  public async Task<ActionResult<ProcessorVersionResp>> GetVersion(string username, string processorName, ulong ver,
    bool bumpDownload)
  {
    var processor = await service.GetVersion(username, processorName, ver, bumpDownload)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(processor,
      new EntityNotFound("Processor not found", typeof(ProcessorVersion), $"{username}/{processorName}:{ver}"));
  }

  [HttpGet("slug/{username}/{processorName}/versions/latest")]
  public async Task<ActionResult<ProcessorVersionResp>> GetVersion(string username, string processorName, bool bumpDownload)
  {
    var processor = await service.GetVersion(username, processorName, bumpDownload)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(processor,
      new EntityNotFound("Processor not found", typeof(ProcessorVersion), $"{username}/{processorName}"));
  }

  [HttpGet("id/{userId}/{processorId:guid}/versions/{ver}")]
  public async Task<ActionResult<ProcessorVersionResp>> GetVersion(string userId, Guid processorId, ulong ver)
  {
    var processor = await service.GetVersion(userId, processorId, ver)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(processor,
      new EntityNotFound("Processor not found", typeof(ProcessorVersion), $"{userId}/{processorId}:{ver}"));
  }



  [Authorize, HttpPost("slug/{username}/{processorName}/versions")]
  public async Task<ActionResult<ProcessorVersionPrincipalResp>> CreateVersion(string username, string processorName,
    [FromBody] CreateProcessorVersionReq req)
  {
    var sub = this.Sub();
    var version = await userService
      .GetByUsername(username)
      .ThenAwait(x => Task.FromResult(x?.Principal.Id == sub), Errors.MapAll)
      .ThenAwait(async x =>
      {
        if (x)
        {
          return await createProcessorVersionReqValidator
            .ValidateAsyncResult(req, "Invalid CreateProcessorVersionReq")
            .ThenAwait(c => service.CreateVersion(username, processorName, c.ToDomain().Item2, c.ToDomain().Item1))
            .Then(c => c?.ToResp(), Errors.MapAll);
        }

        return new Unauthorized("You are not authorized to create a processor for this user")
          .ToException();
      });
    return this.ReturnNullableResult(version,
      new EntityNotFound("Processor not found", typeof(ProcessorPrincipal), $"{username}/{processorName}"));
  }

  [Authorize, HttpPost("id/{userId}/{processorId:guid}/versions")]
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

    var version = await createProcessorVersionReqValidator
      .ValidateAsyncResult(req, "Invalid CreateProcessorVersionReq")
      .ThenAwait(x => service.CreateVersion(userId, processorId, x.ToDomain().Item2, x.ToDomain().Item1))
      .Then(x => x?.ToResp(), Errors.MapAll);

    return this.ReturnNullableResult(version,
      new EntityNotFound("Processor not found", typeof(ProcessorPrincipal), $"{userId}/{processorId}"));
  }

  [Authorize, HttpPut("id/{userId}/{processorId:guid}/versions/{ver}")]
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

    var version = await updateProcessorVersionReqValidator
      .ValidateAsyncResult(req, "Invalid UpdateProcessorVersionReq")
      .ThenAwait(x => service.UpdateVersion(userId, processorId, ver, x.ToDomain()))
      .Then(x => x?.ToResp(), Errors.MapAll);

    return this.ReturnNullableResult(version,
      new EntityNotFound("Processor not found", typeof(ProcessorPrincipal), $"{userId}/{processorId}"));
  }

  [Authorize, HttpPost("push/{username}")]
  public async Task<ActionResult<ProcessorVersionPrincipalResp>> CreateVersion(string username, [FromBody] PushProcessorReq req)
  {
    var sub = this.Sub();
    var version = await userService
      .GetByUsername(username)
      .ThenAwait(x => Task.FromResult(x?.Principal.Id == sub), Errors.MapAll)
      .ThenAwait(async x =>
      {
        if (x)
        {
          return await processorReqValidator
            .ValidateAsyncResult(req, "Invalid PushProcessorReq")
            .Then(push => push.ToDomain(), Errors.MapAll)
            .ThenAwait(domain =>
            {
              var (record, metadata, vRecord, vProperty) = domain;
              return service.Push(username, record, metadata, vRecord, vProperty);
            })
            .Then(c => c?.ToResp(), Errors.MapAll);
        }

        return new Unauthorized("You are not authorized to create a processor for this user")
          .ToException();
      });

    return this.ReturnNullableResult(version,
      new EntityNotFound("Processor not found", typeof(ProcessorPrincipal), $"{username}/{req.Name}"));
  }

}
