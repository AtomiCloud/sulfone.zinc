using App.Modules.Common;
using App.StartUp.Options;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace App.Modules.System;

[ApiVersionNeutral]
[ApiController]
[Route("/")]
public class SystemController : AtomiControllerBase
{
  private readonly IOptionsSnapshot<AppOption> _app;

  public SystemController(IOptionsSnapshot<AppOption> app)
  {
    this._app = app;
  }

  [HttpGet]
  public ActionResult<object> SystemInfo()
  {
    var v = this._app.Value;
    return this.Ok(new
    {
      v.Landscape,
      v.Platform,
      v.Service,
      v.Module,
      v.Version,
      Status = "OK",
      TimeStamp = DateTime.UtcNow,
    });
  }
}
