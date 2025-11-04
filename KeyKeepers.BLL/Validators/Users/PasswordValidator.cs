using FluentValidation;
using KeyKeepers.BLL.Constants;

namespace KeyKeepers.BLL.Validators.Users;

public class PasswordValidator : AbstractValidator<string>
{
    public PasswordValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage(UserConstants.PasswordRequiredErrorMessage)
            .MinimumLength(UserConstants.MinPasswordLength).WithMessage(UserConstants.PasswordLengthErrorMessage)
            .Matches("[A-Z]").WithMessage(UserConstants.PasswordUppercaseLetterErrorMessage)
            .Matches("[0-9]").WithMessage(UserConstants.PasswordDigitErrorMessage)
            .Matches("[^a-zA-Z0-9]").WithMessage(UserConstants.PasswordSpecialCharacterErrorMessage);
    }
}
