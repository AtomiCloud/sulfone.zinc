using System.Text.Json;
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

public class PluginReferenceReqValidator : AbstractValidator<PluginReferenceReq>
{
  public PluginReferenceReqValidator()
  {
    this.RuleFor(x => x.Username).NotNull().UsernameValid();
    this.RuleFor(x => x.Name).NotNull().UsernameValid();
  }
}

public class ProcessorReferenceReqValidator : AbstractValidator<ProcessorReferenceReq>
{
  public ProcessorReferenceReqValidator()
  {
    this.RuleFor(x => x.Username).NotNull().UsernameValid();
    this.RuleFor(x => x.Name).NotNull().UsernameValid();
  }
}

public class TemplateReferenceReqValidator : AbstractValidator<TemplateReferenceReq>
{
  public TemplateReferenceReqValidator()
  {
    this.RuleFor(x => x.Username).NotNull().UsernameValid();
    this.RuleFor(x => x.Name).NotNull().UsernameValid();
    this.RuleFor(x => x.PresetAnswers)
      .Must(a => a.ValueKind == JsonValueKind.Object)
      .WithMessage("presetAnswers must be a valid JSON object.");
  }
}

public class ResolverReferenceReqValidator : AbstractValidator<ResolverReferenceReq>
{
  public ResolverReferenceReqValidator()
  {
    this.RuleFor(x => x.Username).NotNull().UsernameValid();
    this.RuleFor(x => x.Name).NotNull().UsernameValid();
    this.RuleFor(x => x.Config)
      .Must(c => c.ValueKind == JsonValueKind.Object)
      .WithMessage("config must be a valid JSON object.");
    this.RuleFor(x => x.Files).NotNull();
    this.RuleForEach(x => x.Files!)
      .NotNull()
      .Must(file => !string.IsNullOrWhiteSpace(file))
      .WithMessage("file pattern cannot be empty or whitespace.");
  }
}

public class CreateTemplateVersionReqValidator : AbstractValidator<CreateTemplateVersionReq>
{
  public CreateTemplateVersionReqValidator()
  {
    this.RuleFor(x => x.Description).DescriptionValid();
    this.RuleFor(x => x.Properties!)
      .SetValidator(new TemplatePropertyReqValidator())
      .Unless(x => x.Properties == null);
    this.RuleForEach(x => x.Plugins!)
      .SetValidator(new PluginReferenceReqValidator())
      .When(x => x.Plugins != null);
    this.RuleForEach(x => x.Processors!)
      .SetValidator(new ProcessorReferenceReqValidator())
      .When(x => x.Processors != null);
    this.RuleForEach(x => x.Templates!)
      .SetValidator(new TemplateReferenceReqValidator())
      .When(x => x.Templates != null);
    this.RuleForEach(x => x.Resolvers!)
      .SetValidator(new ResolverReferenceReqValidator())
      .When(x => x.Resolvers != null);
  }
}

public class TemplatePropertyReqValidator : AbstractValidator<TemplatePropertyReq>
{
  public TemplatePropertyReqValidator()
  {
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
    this.RuleFor(x => x.Properties!)
      .SetValidator(new TemplatePropertyReqValidator())
      .Unless(x => x.Properties == null);
    this.RuleForEach(x => x.Plugins!)
      .SetValidator(new PluginReferenceReqValidator())
      .When(x => x.Plugins != null);
    this.RuleForEach(x => x.Processors!)
      .SetValidator(new ProcessorReferenceReqValidator())
      .When(x => x.Processors != null);
    this.RuleForEach(x => x.Templates!)
      .SetValidator(new TemplateReferenceReqValidator())
      .When(x => x.Templates != null);
    this.RuleForEach(x => x.Resolvers!)
      .SetValidator(new ResolverReferenceReqValidator())
      .When(x => x.Resolvers != null);
  }
}
