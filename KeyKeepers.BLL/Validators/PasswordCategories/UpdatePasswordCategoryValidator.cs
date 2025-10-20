using FluentValidation;
using KeyKeepers.BLL.Commands.PasswordCategory.Update;

namespace KeyKeepers.BLL.Validators.PasswordCategories;

public class UpdatePasswordCategoryValidator : AbstractValidator<UpdatePrivateCategoryCommand>
{
    public UpdatePasswordCategoryValidator(PrivateCategoryValidator privateCategoryValidator)
    {
        RuleFor(x => x.RequestDto).SetValidator(privateCategoryValidator);
    }
}
