using FluentValidation;
using KeyKeepers.BLL.Commands.Passwords.Create;

namespace KeyKeepers.BLL.Validators.Passwords;

public class CreatePasswordValidator : AbstractValidator<CreatePasswordCommand>
{
    public CreatePasswordValidator(BasePasswordValidator passwordValidator)
    {
        RuleFor(x => x.Request).SetValidator(passwordValidator);
    }
}
