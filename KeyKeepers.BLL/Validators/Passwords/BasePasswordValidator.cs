using FluentValidation;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Passwords;

namespace KeyKeepers.BLL.Validators.Passwords;

public class BasePasswordValidator : AbstractValidator<CreatePasswordRequest>
{
    public BasePasswordValidator()
    {
        RuleFor(x => x.AppName)
            .NotEmpty().WithMessage(PasswordConstants.AppNameRequiredError)
            .MinimumLength(PasswordConstants.MinAppNameLenght).WithMessage(PasswordConstants.MinAppNameLenghtError)
            .MaximumLength(PasswordConstants.MaxAppNameLenght).WithMessage(PasswordConstants.MaxAppNameLenghtError);

        RuleFor(x => x.Password).NotEmpty().WithMessage(PasswordConstants.PasswordRequiredError)
            .MaximumLength(PasswordConstants.MaxPasswordLenght).WithMessage(PasswordConstants.MaxPasswordLenghtError);

        RuleFor(x => x.Login)
            .NotEmpty().WithMessage(PasswordConstants.LoginRequiredError)
            .MaximumLength(PasswordConstants.MaxLoginLenght).WithMessage(PasswordConstants.MaxLoginLenghtError);
    }
}
