using App.Error;
using App.StartUp.Options;
using App.StartUp.Registry;

namespace App.StartUp.Services;

public static class ProblemDetailsService
{
  public static IServiceCollection AddProblemDetailsService(
    this IServiceCollection service,
    ErrorPortalOption ep,
    AppOption ap
  )
  {
    service.AddProblemDetails(x =>
      x.CustomizeProblemDetails = context =>
      {
        if (context.HttpContext.Items[Constants.ProblemContextKey] is not IDomainProblem problem)
          return;
        context.ProblemDetails.Detail = problem.Detail;
        context.ProblemDetails.Title = problem.Title;
        if (ep.Enabled)
        {
          context.ProblemDetails.Type =
            $"{ep.Scheme}://{ep.Host}/docs/{ap.Landscape}/{ap.Platform}/{ap.Service}/{ap.Module}/{problem.Version}/{problem.Id}";
        }
        context.ProblemDetails.Extensions["data"] = problem;
      }
    );
    return service;
  }
}
