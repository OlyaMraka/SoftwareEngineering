using FluentValidation;
using KeyKeepers.BLL.DTOs.PasswordCategories;
using KeyKeepers.BLL.Constants;

namespace KeyKeepers.BLL.Validators.PasswordCategories;

public class PrivateCategoryValidator : AbstractValidator<CreatePrivateCategoryDto>
{
    public PrivateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(PasswordCategoriesConstants.NameRequiredErrorMessage)
            .MinimumLength(PasswordCategoriesConstants.MinNameLength)
            .WithMessage(PasswordCategoriesConstants.MinNameLengthErrorMessage)
            .MaximumLength(PasswordCategoriesConstants.MaxNameLength)
            .WithMessage(PasswordCategoriesConstants.MaxNameLengthErrorMessage);
    }
}
