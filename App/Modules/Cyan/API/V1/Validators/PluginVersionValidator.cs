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
    this.RuleFor(x => x.DockerTag)
      .TagValid();
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

public class PushPluginReqValidator : AbstractValidator<PushPluginReq>
{
  public PushPluginReqValidator()
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
