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
    this.RuleFor(x => x.Description).DescriptionValid();
    this.RuleFor(x => x.BlobDockerReference).DockerReferenceValid();
    this.RuleFor(x => x.BlobDockerTag).TagValid();
    this.RuleFor(x => x.TemplateDockerReference).DockerReferenceValid();
    this.RuleFor(x => x.TemplateDockerTag).TagValid();
  }
}

public class UpdateTemplateVersionReqValidator : AbstractValidator<UpdateTemplateVersionReq>
{
  public UpdateTemplateVersionReqValidator()
  {
    this.RuleFor(x => x.Description).DescriptionValid();
  }
}

public class PushTemplateReqValidator : AbstractValidator<PushTemplateReq>
{
  public PushTemplateReqValidator()
  {
    this.RuleFor(x => x.Name).NotNull().UsernameValid();
    this.RuleFor(x => x.Project).UrlValid();
    this.RuleFor(x => x.Source).UrlValid();
    this.RuleFor(x => x.Email).EmailAddress();
    this.RuleForEach(x => x.Tags).UsernameValid().NotNull();
    this.RuleFor(x => x.Tags).NotNull();
    this.RuleFor(x => x.Description).DescriptionValid();
    this.RuleFor(x => x.Readme).NotNull();
    this.RuleFor(x => x.VersionDescription).DescriptionValid();
    this.RuleFor(x => x.BlobDockerReference).DockerReferenceValid();
    this.RuleFor(x => x.BlobDockerTag).TagValid();
    this.RuleFor(x => x.TemplateDockerReference).DockerReferenceValid();
    this.RuleFor(x => x.TemplateDockerTag).TagValid();

    this.RuleForEach(x => x.Plugins)
      .Must(p => p.Version != 0)
      .WithMessage(
        "Plugin reference '{PropertyValue}' has version 0, which is not allowed. Use null for latest version or specify a concrete version."
      );

    this.RuleForEach(x => x.Processors)
      .Must(p => p.Version != 0)
      .WithMessage(
        "Processor reference '{PropertyValue}' has version 0, which is not allowed. Use null for latest version or specify a concrete version."
      );
  }
}
