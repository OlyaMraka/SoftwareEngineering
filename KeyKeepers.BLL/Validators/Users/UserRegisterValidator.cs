using FluentValidation;
using KeyKeepers.BLL.Commands.Users.Create;
using KeyKeepers.BLL.Constants;

namespace KeyKeepers.BLL.Validators.Users;

public class UserRegisterValidator : AbstractValidator<CreateUserCommand>
{
    public UserRegisterValidator()
    {
        RuleFor(x => x.RegisterDto.Name)
            .NotEmpty().WithMessage(UserConstants.NameRequiredErrorMessage)
            .MaximumLength(UserConstants.MaxNameLength).WithMessage(UserConstants.MaxNameLengthErrorMessage)
            .MinimumLength(UserConstants.MinNameLength).WithMessage(UserConstants.MinNameLengthErrorMessage);

        RuleFor(x => x.RegisterDto.Surname)
            .NotEmpty().WithMessage(UserConstants.SurnameRequiredErrorMessage)
            .MaximumLength(UserConstants.MaxSurnameLength).WithMessage(UserConstants.MaxSurnameLengthErrorMessage)
            .MinimumLength(UserConstants.MinSurnameLength).WithMessage(UserConstants.MinSurnameLengthErrorMessage);

        RuleFor(x => x.RegisterDto.Email)
            .NotEmpty().WithMessage(UserConstants.EmailRequiredErrorMessage)
            .MaximumLength(UserConstants.MaxEmailLength).WithMessage(UserConstants.MaxEmailLengthErrorMessage)
            .MinimumLength(UserConstants.MinEmailLength).WithMessage(UserConstants.MinEmailLengthErrorMessage);

        RuleFor(x => x.RegisterDto.UserName)
            .NotEmpty().WithMessage(UserConstants.UserNameRequiredErrorMessage)
            .MaximumLength(UserConstants.MaxUserNameLength).WithMessage(UserConstants.MaxUserNameErrorMessage)
            .MinimumLength(UserConstants.MinUserNameLength).WithMessage(UserConstants.MinUserNameErrorMessage);

        RuleFor(x => x.RegisterDto.Password)
            .NotEmpty().WithMessage(UserConstants.PasswordRequiredErrorMessage)
            .MinimumLength(UserConstants.MinPasswordLength).WithMessage(UserConstants.PasswordLengthErrorMessage)
            .Matches("[A-Z]").WithMessage(UserConstants.PasswordUppercaseLetterErrorMessage)
            .Matches("[0-9]").WithMessage(UserConstants.PasswordDigitErrorMessage)
            .Matches("[^a-zA-Z0-9]").WithMessage(UserConstants.PasswordSpecialCharacterErrorMessage);
    }
}
