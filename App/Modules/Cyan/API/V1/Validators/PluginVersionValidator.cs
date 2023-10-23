using App.Modules.Cyan.API.V1.Models;
using App.Utility;
using FluentValidation;

namespace App.Modules.Cyan.API.V1.Validators;

public class SearchPluginVersionQueryValidator : AbstractValidator<SearchPluginVersionQuery>
{
  public SearchPluginVersionQueryValidator()
  {
    this.RuleFor(x => x.Skip).Skip();
    this.RuleFor(x => x.Limit).Limit();
  }
}

public class CreatePluginVersionReqValidator : AbstractValidator<CreatePluginVersionReq>
{
  public CreatePluginVersionReqValidator()
  {
    this.RuleFor(x => x.Description)
      .DescriptionValid();
    this.RuleFor(x => x.DockerReference)
      .DockerReferenceValid();
    this.RuleFor(x => x.DockerSha)
      .ShaValid();
  }
}

public class UpdatePluginVersionReqValidator : AbstractValidator<UpdatePluginVersionReq>
{
  public UpdatePluginVersionReqValidator()
  {
    this.RuleFor(x => x.Description)
      .DescriptionValid();
  }
}
