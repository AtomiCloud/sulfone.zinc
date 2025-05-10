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
public class TemplateController(
  ITemplateService service,
  CreateTemplateReqValidator createTemplateReqValidator,
  UpdateTemplateReqValidator updateTemplateReqValidator,
  SearchTemplateQueryValidator searchTemplateQueryValidator,
  CreateTemplateVersionReqValidator createTemplateVersionReqValidator,
  UpdateTemplateVersionReqValidator updateTemplateVersionReqValidator,
  SearchTemplateVersionQueryValidator searchTemplateVersionQueryValidator,
  IUserService userService,
  PushTemplateReqValidator templateReqValidator,
  ILogger<TemplateController> logger
) : AtomiControllerBase
{
  [HttpGet]
  public async Task<ActionResult<IEnumerable<TemplatePrincipalResp>>> Search(
    [FromQuery] SearchTemplateQuery query
  )
  {
    var templates = await searchTemplateQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchTemplateQuery")
      .ThenAwait(x => service.Search(x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp()).ToResult());

    return this.ReturnResult(templates);
  }

  [HttpGet("id/{userId}/{templateId:guid}")]
  public async Task<ActionResult<TemplateResp>> Get(string userId, Guid templateId)
  {
    var template = await service.Get(userId, templateId).Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      template,
      new EntityNotFound("Template not found", typeof(TemplatePrincipal), templateId.ToString())
    );
  }

  [HttpGet("slug/{username}/{name}")]
  public async Task<ActionResult<TemplateResp>> Get(string username, string name)
  {
    var template = await service.Get(username, name).Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      template,
      new EntityNotFound("Template not found", typeof(TemplatePrincipal), $"{username}/{name}")
    );
  }

  [Authorize, HttpPost("id/{userId}")]
  public async Task<ActionResult<TemplatePrincipalResp>> Create(
    string userId,
    [FromBody] CreateTemplateReq req
  )
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<TemplatePrincipalResp> e = new Unauthorized(
        "You are not authorized to create a template for this user"
      ).ToException();
      return this.ReturnResult(e);
    }

    var template = await createTemplateReqValidator
      .ValidateAsyncResult(req, "Invalid CreateTemplateReq")
      .ThenAwait(x => service.Create(userId, x.ToDomain().Item1, x.ToDomain().Item2))
      .Then(x => x.ToResp(), Errors.MapAll);
    return this.ReturnResult(template);
  }

  [Authorize, HttpPut("id/{userId}/{templateId:guid}")]
  public async Task<ActionResult<TemplatePrincipalResp>> Update(
    string userId,
    Guid templateId,
    [FromBody] UpdateTemplateReq req
  )
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<TemplatePrincipalResp> e = new Unauthorized(
        "You are not authorized to create a template for this user"
      ).ToException();
      return this.ReturnResult(e);
    }

    var template = await updateTemplateReqValidator
      .ValidateAsyncResult(req, "Invalid UpdateTemplateReq")
      .ThenAwait(x => service.Update(userId, templateId, x.ToDomain()))
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      template,
      new EntityNotFound("Template not found", typeof(TemplatePrincipal), templateId.ToString())
    );
  }

  [Authorize, HttpPost("slug/{username}/{templateName}/like/{likerId}/{like:bool}")]
  public async Task<ActionResult<Unit>> Like(
    string username,
    string templateName,
    string likerId,
    bool like
  )
  {
    var sub = this.Sub();
    if (sub != likerId)
    {
      Result<Unit> e = new Unauthorized(
        "You are not authorized to like this template"
      ).ToException();
      return this.ReturnResult(e);
    }

    var template = await service
      .Like(likerId, username, templateName, like)
      .Then(x => x.ToResult());
    return this.ReturnUnitNullableResult(
      template,
      new EntityNotFound(
        "Template not found",
        typeof(TemplatePrincipal),
        $"{username}/{templateName}"
      )
    );
  }

  [Authorize(Policy = AuthPolicies.OnlyAdmin), HttpDelete("id/{userId}/{templateId:guid}")]
  public async Task<ActionResult<Unit>> Delete(string userId, Guid templateId)
  {
    var template = await service.Delete(userId, templateId).Then(x => x.ToResult());
    return this.ReturnUnitNullableResult(
      template,
      new EntityNotFound("Template not found", typeof(TemplatePrincipal), $"{userId}/{templateId}")
    );
  }

  [HttpGet("slug/{username}/{templateName}/versions")]
  public async Task<ActionResult<IEnumerable<TemplateVersionPrincipalResp>>> SearchVersion(
    string username,
    string templateName,
    [FromQuery] SearchTemplateVersionQuery query
  )
  {
    var templates = await searchTemplateVersionQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchTemplateVersionQuery")
      .ThenAwait(x => service.SearchVersion(username, templateName, x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp()).ToResult());
    return this.ReturnResult(templates);
  }

  [HttpGet("id/{userId}/{templateId:guid}/versions")]
  public async Task<ActionResult<IEnumerable<TemplateVersionPrincipalResp>>> SearchVersion(
    string userId,
    Guid templateId,
    [FromQuery] SearchTemplateVersionQuery query
  )
  {
    var templates = await searchTemplateVersionQueryValidator
      .ValidateAsyncResult(query, "Invalid SearchTemplateVersionQuery")
      .ThenAwait(x => service.SearchVersion(userId, templateId, x.ToDomain()))
      .Then(x => x.Select(u => u.ToResp()).ToResult());
    return this.ReturnResult(templates);
  }

  [HttpGet("slug/{username}/{templateName}/versions/{ver}")]
  public async Task<ActionResult<TemplateVersionResp>> GetVersion(
    string username,
    string templateName,
    ulong ver,
    bool bumpDownload
  )
  {
    var template = await service
      .GetVersion(username, templateName, ver, bumpDownload)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      template,
      new EntityNotFound(
        "Template not found",
        typeof(TemplateVersionResp),
        $"{username}/{templateName}:{ver}"
      )
    );
  }

  [HttpGet("slug/{username}/{templateName}/versions/latest")]
  public async Task<ActionResult<TemplateVersionResp>> GetVersion(
    string username,
    string templateName,
    bool bumpDownload
  )
  {
    var template = await service
      .GetVersion(username, templateName, bumpDownload)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      template,
      new EntityNotFound(
        "Template not found",
        typeof(TemplateVersionResp),
        $"{username}/{templateName}"
      )
    );
  }

  [HttpGet("id/{userId}/{templateId:guid}/versions/{ver}")]
  public async Task<ActionResult<TemplateVersionResp>> GetVersion(
    string userId,
    Guid templateId,
    ulong ver
  )
  {
    var template = await service
      .GetVersion(userId, templateId, ver)
      .Then(x => x?.ToResp(), Errors.MapAll);
    return this.ReturnNullableResult(
      template,
      new EntityNotFound(
        "Template not found",
        typeof(TemplateVersionPrincipal),
        $"{userId}/{templateId}:{ver}"
      )
    );
  }

  [Authorize, HttpPost("slug/{username}/{templateName}/versions")]
  public async Task<ActionResult<TemplateVersionPrincipalResp>> CreateVersion(
    string username,
    string templateName,
    [FromBody] CreateTemplateVersionReq req
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
          return await createTemplateVersionReqValidator
            .ValidateAsyncResult(req, "Invalid CreateTemplateVersionReq")
            .ThenAwait(c =>
              service.CreateVersion(
                username,
                templateName,
                c.ToRecord(),
                c.Properties?.ToProperty(),
                c.Processors.Select(p => p.ToDomain()),
                c.Plugins.Select(p => p.ToDomain()),
                c.Templates.Select(t => t.ToDomain())
              )
            )
            .Then(c => c?.ToResp(), Errors.MapAll);
        }

        return new Unauthorized(
          "You are not authorized to create a template for this user"
        ).ToException();
      });
    return this.ReturnNullableResult(
      version,
      new EntityNotFound(
        "Template not found",
        typeof(TemplatePrincipal),
        $"{username}/{templateName}"
      )
    );
  }

  [Authorize, HttpPost("id/{userId}/{templateId:guid}/versions")]
  public async Task<ActionResult<TemplateVersionPrincipalResp>> CreateVersion(
    string userId,
    Guid templateId,
    [FromBody] CreateTemplateVersionReq req
  )
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<TemplateVersionPrincipalResp> e = new Unauthorized(
        "You are not authorized to create a template for this user"
      ).ToException();
      return this.ReturnResult(e);
    }

    var version = await createTemplateVersionReqValidator
      .ValidateAsyncResult(req, "Invalid CreateTemplateVersionReq")
      .ThenAwait(c =>
        service.CreateVersion(
          userId,
          templateId,
          c.ToRecord(),
          c.Properties?.ToProperty(),
          c.Processors.Select(p => p.ToDomain()),
          c.Plugins.Select(p => p.ToDomain()),
          c.Templates.Select(t => t.ToDomain())
        )
      )
      .Then(x => x?.ToResp(), Errors.MapAll);

    return this.ReturnNullableResult(
      version,
      new EntityNotFound("Template not found", typeof(TemplatePrincipal), $"{userId}/{templateId}")
    );
  }

  [Authorize, HttpPut("id/{userId}/{templateId:guid}/versions/{ver}")]
  public async Task<ActionResult<TemplateVersionPrincipalResp>> UpdateVersion(
    string userId,
    Guid templateId,
    ulong ver,
    [FromBody] UpdateTemplateVersionReq req
  )
  {
    var sub = this.Sub();
    if (sub != userId)
    {
      Result<TemplateVersionPrincipalResp> e = new Unauthorized(
        "You are not authorized to create a template for this user"
      ).ToException();
      return this.ReturnResult(e);
    }

    var version = await updateTemplateVersionReqValidator
      .ValidateAsyncResult(req, "Invalid UpdateTemplateVersionReq")
      .ThenAwait(x => service.UpdateVersion(userId, templateId, ver, x.ToDomain()))
      .Then(x => x?.ToResp(), Errors.MapAll);

    return this.ReturnNullableResult(
      version,
      new EntityNotFound("Template not found", typeof(TemplatePrincipal), $"{userId}/{templateId}")
    );
  }

  [Authorize, HttpPost("push/{username}")]
  public async Task<ActionResult<TemplateVersionPrincipalResp>> CreateVersion(
    string username,
    [FromBody] PushTemplateReq req
  )
  {
    logger.LogInformation("Version, Template: {Template}", req.ToJson());
    var sub = this.Sub();
    var version = await userService
      .GetByUsername(username)
      .ThenAwait(x => Task.FromResult(x?.Principal.Id == sub), Errors.MapAll)
      .ThenAwait(async x =>
      {
        if (x)
        {
          return await templateReqValidator
            .ValidateAsyncResult(req, "Invalid PushTemplateReq")
            .Then(push => push.ToDomain(), Errors.MapAll)
            .ThenAwait(domain =>
            {
              var (record, metadata, vRecord, vProperty) = domain;
              return service.Push(
                username,
                record,
                metadata,
                vRecord,
                vProperty,
                req.Processors.Select(p => p.ToDomain()),
                req.Plugins.Select(p => p.ToDomain()),
                req.Templates.Select(t => t.ToDomain())
              );
            })
            .Then(c => c?.ToResp(), Errors.MapAll);
        }

        return new Unauthorized(
          "You are not authorized to create a template for this user"
        ).ToException();
      });

    return this.ReturnNullableResult(
      version,
      new EntityNotFound("Template not found", typeof(TemplatePrincipal), $"{username}/{req.Name}")
    );
  }
}
