using FluentValidation;
using KeyKeepers.BLL.Commands.Passwords.Update;

namespace KeyKeepers.BLL.Validators.Passwords;

public class UpdatePasswordValidator : AbstractValidator<UpdatePasswordCommand>
{
    public UpdatePasswordValidator(BasePasswordValidator passwordValidator)
    {
        RuleFor(x => x.Request).SetValidator(passwordValidator);
    }
}
