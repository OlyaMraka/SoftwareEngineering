using FluentValidation;
using KeyKeepers.BLL.Commands.PasswordCategory.Create;
using KeyKeepers.BLL.Constants;

namespace KeyKeepers.BLL.Validators.PasswordCategories;

public class CreatePrivateCategoryValidator : AbstractValidator<CreatePrivateCategoryCommand>
{
    public CreatePrivateCategoryValidator()
    {
        RuleFor(x => x.RequestDto.Name)
            .NotEmpty()
            .WithMessage(PasswordCategoriesConstants.NameRequiredErrorMessage)
            .MinimumLength(PasswordCategoriesConstants.MinNameLength)
            .WithMessage(PasswordCategoriesConstants.MinNameLengthErrorMessage)
            .MaximumLength(PasswordCategoriesConstants.MaxNameLength)
            .WithMessage(PasswordCategoriesConstants.MaxNameLengthErrorMessage);
    }
}
