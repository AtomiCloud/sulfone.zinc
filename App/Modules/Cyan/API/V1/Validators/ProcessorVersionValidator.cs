using App.Modules.Cyan.API.V1.Models;
using App.Utility;
using FluentValidation;

namespace App.Modules.Cyan.API.V1.Validators;

public class SearchProcessorVersionQueryValidator : AbstractValidator<SearchProcessorVersionQuery>
{
  public SearchProcessorVersionQueryValidator()
  {
    this.RuleFor(x => x.Skip).Skip();
    this.RuleFor(x => x.Limit).Limit();
  }
}

public class CreateProcessorVersionReqValidator : AbstractValidator<CreateProcessorVersionReq>
{
  public CreateProcessorVersionReqValidator()
  {
    this.RuleFor(x => x.Description)
      .DescriptionValid();
    this.RuleFor(x => x.DockerReference)
      .DockerReferenceValid();
    this.RuleFor(x => x.DockerSha)
      .ShaValid();
  }
}

public class UpdateProcessorVersionReqValidator : AbstractValidator<UpdateProcessorVersionReq>
{
  public UpdateProcessorVersionReqValidator()
  {
    this.RuleFor(x => x.Description)
      .DescriptionValid();
  }
}
