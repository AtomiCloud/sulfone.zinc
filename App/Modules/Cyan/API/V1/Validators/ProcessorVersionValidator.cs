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
    this.RuleFor(x => x.DockerTag)
      .TagValid();
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

public class PushProcessorReqValidator : AbstractValidator<PushProcessorReq>
{
  public PushProcessorReqValidator()
  {
    this.RuleFor(x => x.Name)
      .NotNull()
      .UsernameValid();
    this.RuleFor(x => x.Project)
      .UrlValid();
    this.RuleFor(x => x.Source)
      .UrlValid();
    this.RuleFor(x => x.Email)
      .EmailAddress();
    this.RuleForEach(x => x.Tags)
      .UsernameValid()
      .NotNull();
    this.RuleFor(x => x.Tags)
      .NotNull();
    this.RuleFor(x => x.Description)
      .DescriptionValid();
    this.RuleFor(x => x.Readme)
      .NotNull();
    this.RuleFor(x => x.VersionDescription)
      .DescriptionValid();
    this.RuleFor(x => x.DockerReference)
      .DockerReferenceValid();
    this.RuleFor(x => x.DockerTag)
      .TagValid();
  }
}
