using App.Modules.Cyan.API.V1.Models;
using App.Utility;
using FluentValidation;

namespace App.Modules.Cyan.API.V1.Validators;

public class SearchTemplateVersionQueryValidator : AbstractValidator<SearchTemplateVersionQuery>
{
  public SearchTemplateVersionQueryValidator()
  {
    this.RuleFor(x => x.Skip).Skip();
    this.RuleFor(x => x.Limit).Limit();
  }
}

public class CreateTemplateVersionReqValidator : AbstractValidator<CreateTemplateVersionReq>
{
  public CreateTemplateVersionReqValidator()
  {
    this.RuleFor(x => x.Description)
      .DescriptionValid();
    this.RuleFor(x => x.BlobDockerReference)
      .DockerReferenceValid();
    this.RuleFor(x => x.BlobDockerSha)
      .ShaValid();
    this.RuleFor(x => x.TemplateDockerReference)
      .DockerReferenceValid();
    this.RuleFor(x => x.TemplateDockerSha)
      .ShaValid();
  }
}

public class UpdateTemplateVersionReqValidator : AbstractValidator<UpdateTemplateVersionReq>
{
  public UpdateTemplateVersionReqValidator()
  {
    this.RuleFor(x => x.Description)
      .DescriptionValid();
  }
}
