using FluentValidation;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.BLL.Constants;

namespace KeyKeepers.BLL.Validators.Users;

public class BaseUserValidator : AbstractValidator<UserRegisterDto>
{
    public BaseUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(UserConstants.NameRequiredErrorMessage)
            .MaximumLength(UserConstants.MaxNameLength).WithMessage(UserConstants.MaxNameLengthErrorMessage)
            .MinimumLength(UserConstants.MinNameLength).WithMessage(UserConstants.MinNameLengthErrorMessage);

        RuleFor(x => x.Surname)
            .NotEmpty().WithMessage(UserConstants.SurnameRequiredErrorMessage)
            .MaximumLength(UserConstants.MaxSurnameLength).WithMessage(UserConstants.MaxSurnameLengthErrorMessage)
            .MinimumLength(UserConstants.MinSurnameLength).WithMessage(UserConstants.MinSurnameLengthErrorMessage);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(UserConstants.EmailRequiredErrorMessage)
            .MaximumLength(UserConstants.MaxEmailLength).WithMessage(UserConstants.MaxEmailLengthErrorMessage)
            .MinimumLength(UserConstants.MinEmailLength).WithMessage(UserConstants.MinEmailLengthErrorMessage);

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage(UserConstants.UserNameRequiredErrorMessage)
            .MaximumLength(UserConstants.MaxUserNameLength).WithMessage(UserConstants.MaxUserNameErrorMessage)
            .MinimumLength(UserConstants.MinUserNameLength).WithMessage(UserConstants.MinUserNameErrorMessage);
    }
}
