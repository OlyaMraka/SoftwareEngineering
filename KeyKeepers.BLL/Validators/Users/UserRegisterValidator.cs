using FluentValidation;
using KeyKeepers.BLL.Commands.Users.Create;

namespace KeyKeepers.BLL.Validators.Users;

public class UserRegisterValidator : AbstractValidator<CreateUserCommand>
{
    public UserRegisterValidator(BaseUserValidator userValidator)
    {
        RuleFor(x => x.RegisterDto).SetValidator(userValidator);
    }
}
