using App.Modules.Cyan.API.V1.Models;
using App.Utility;
using FluentValidation;

namespace App.Modules.Cyan.API.V1.Validators;

public class SearchResolverQueryValidator : AbstractValidator<SearchResolverQuery>
{
  public SearchResolverQueryValidator()
  {
    this.RuleFor(x => x.Skip).Skip();
    this.RuleFor(x => x.Limit).Limit();
  }
}

public class CreateResolverReqValidator : AbstractValidator<CreateResolverReq>
{
  public CreateResolverReqValidator()
  {
    this.RuleFor(x => x.Name).NotNull().UsernameValid();
    this.RuleFor(x => x.Project).UrlValid();
    this.RuleFor(x => x.Source).UrlValid();
    this.RuleFor(x => x.Email).EmailAddress();
    this.RuleForEach(x => x.Tags).UsernameValid().NotNull();
    this.RuleFor(x => x.Tags).NotNull();
    this.RuleFor(x => x.Description).DescriptionValid();
    this.RuleFor(x => x.Readme).NotNull();
  }
}

public class UpdateResolverReqValidator : AbstractValidator<UpdateResolverReq>
{
  public UpdateResolverReqValidator()
  {
    this.RuleFor(x => x.Project).UrlValid();
    this.RuleFor(x => x.Source).UrlValid();
    this.RuleFor(x => x.Email).EmailAddress();
    this.RuleForEach(x => x.Tags).UsernameValid().NotNull();
    this.RuleFor(x => x.Tags).NotNull();
    this.RuleFor(x => x.Description).DescriptionValid();
    this.RuleFor(x => x.Readme).NotNull();
  }
}
