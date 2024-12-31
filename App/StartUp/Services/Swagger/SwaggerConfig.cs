using System.Text;
using App.StartUp.Options.Swagger;
using App.Utility;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace App.StartUp.Services.Swagger;

#pragma warning disable IDE0290


/// <summary>
/// Configures the Swagger generation options.
/// </summary>
/// <remarks>This allows API versioning to define a Swagger document per API version after the
/// <see cref="IApiVersionDescriptionProvider"/> service has been resolved from the service container.</remarks>
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
  private readonly IApiVersionDescriptionProvider _provider;
  private readonly IOptionsMonitor<OpenApiOption> _swaggerConfig;

  /// <summary>
  /// Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
  /// </summary>
  /// <param name="provider">The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger documents.</param>
  /// <param name="swaggerConfig">The loaded configuration to generate swagger documentation</param>
  public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IOptionsMonitor<OpenApiOption> swaggerConfig)
  {
    this._provider = provider;
    this._swaggerConfig = swaggerConfig;
  }

  /// <inheritdoc />
  public void Configure(SwaggerGenOptions options)
  {
    foreach (var description in this._provider.ApiVersionDescriptions)
    {
      options.SwaggerDoc(description.GroupName, this.CreateInfoForApiVersion(description));
    }
  }

  private OpenApiInfo Info => new()
  {
    Title = this._swaggerConfig.CurrentValue.Title,
    Contact = this._swaggerConfig.CurrentValue.OpenApiContact?.ToDomain(),
    License = this._swaggerConfig.CurrentValue.OpenApiLicense?.ToDomain(),
    TermsOfService = this._swaggerConfig.CurrentValue.TermsOfService?.ToUri(),
  };

  private StringBuilder BuildPolicyDescription(StringBuilder text, SunsetPolicy policy)
  {
    if (policy.Date is { } when)
    {
      text.Append(" The API will be sunset on ")
        .Append(when.Date.ToShortDateString())
        .Append('.');
    }

    if (!policy.HasLinks) return text;
    text.AppendLine();
    foreach (var link in policy.Links)
    {
      if (link.Type != "text/html") continue;
      text.AppendLine();
      if (link.Title.HasValue) text.Append(link.Title.Value).Append(": ");
      text.Append(link.LinkTarget.OriginalString);
    }

    return text;
  }

  private string Description(ApiVersionDescription description)
  {
    var text = new StringBuilder(this._swaggerConfig.CurrentValue.Description);
    if (description.IsDeprecated) text.Append(" This API version has been deprecated.");
    if (description.SunsetPolicy is { } policy) text = this.BuildPolicyDescription(text, policy);
    return text.ToString();
  }

  private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
  {
    var info = this.Info;
    info.Version = description.ApiVersion.ToString();
    info.Description = this.Description(description);
    return info;
  }
}

#pragma warning restore IDE0290
