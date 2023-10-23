using App.Modules.Cyan.API.V1.Models;
using App.Utility;
using FluentValidation;

namespace App.Modules.Cyan.API.V1.Validators;

public class SearchPluginQueryValidator : AbstractValidator<SearchPluginQuery>
{
  public SearchPluginQueryValidator()
  {
    this.RuleFor(x => x.Skip).Skip();
    this.RuleFor(x => x.Limit).Limit();
  }
}

public class CreatePluginReqValidator : AbstractValidator<CreatePluginReq>
{
  public CreatePluginReqValidator()
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
  }
}

public class UpdatePluginReqValidator : AbstractValidator<UpdatePluginReq>
{
  public UpdatePluginReqValidator()
  {
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
  }
}
