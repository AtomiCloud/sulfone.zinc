using App.Utility;
using FluentValidation;

namespace App.Modules.Users.API.V1;

public class CreateUserReqValidator : AbstractValidator<CreateUserReq>
{
  public CreateUserReqValidator()
  {
    this.RuleFor(x => x.Username).NotNull().UsernameValid();
  }
}

public class UpdateUserReqValidator : AbstractValidator<UpdateUserReq>
{
  public UpdateUserReqValidator()
  {
    this.RuleFor(x => x.Username).NotNull().UsernameValid();
  }
}

public class UserSearchQueryValidator : AbstractValidator<SearchUserQuery>
{
  public UserSearchQueryValidator()
  {
    this.RuleFor(x => x.Limit).Limit();
    this.RuleFor(x => x.Skip).Skip();
  }
}

public class CreateTokenReqValidator : AbstractValidator<CreateTokenReq>
{
  public CreateTokenReqValidator()
  {
    this.RuleFor(x => x.Name).NotNull().NotEmpty().NameValid();
  }
}

public class UpdateTokenReqValidator : AbstractValidator<UpdateTokenReq>
{
  public UpdateTokenReqValidator()
  {
    this.RuleFor(x => x.Name).NotNull().NotEmpty().NameValid();
  }
}
