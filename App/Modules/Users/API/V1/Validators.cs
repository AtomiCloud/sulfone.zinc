using FluentValidation;

namespace App.Modules.Users.API.V1;

public class CreateUserReqValidator : AbstractValidator<CreateUserReq>
{
  public CreateUserReqValidator()
  {
    this.RuleFor(x => x.Name)
      .NotNull()
      .MinimumLength(1)
      .MaximumLength(256);

    this.RuleFor(x => x.Email)
      .NotNull()
      .EmailAddress();
  }
}
