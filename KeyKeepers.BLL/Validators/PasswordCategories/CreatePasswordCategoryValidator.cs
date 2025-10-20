using FluentValidation;
using KeyKeepers.BLL.Commands.PasswordCategory.Create;

namespace KeyKeepers.BLL.Validators.PasswordCategories;

public class CreatePasswordCategoryValidator : AbstractValidator<CreatePrivateCategoryCommand>
{
    public CreatePasswordCategoryValidator(PrivateCategoryValidator privateCategoryValidator)
    {
        RuleFor(c => c.RequestDto).SetValidator(privateCategoryValidator);
    }
}
