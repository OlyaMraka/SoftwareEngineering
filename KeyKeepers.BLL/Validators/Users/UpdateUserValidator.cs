using FluentValidation;
using KeyKeepers.BLL.Commands.Users.Update;

namespace KeyKeepers.BLL.Validators.Users;

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator(BaseUserValidator userValidator)
    {
        RuleFor(x => x.RequestDto).SetValidator(userValidator);
    }
}
