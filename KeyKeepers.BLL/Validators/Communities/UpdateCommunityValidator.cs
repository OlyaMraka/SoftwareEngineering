using FluentValidation;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.Commands.Communities.Update;

namespace KeyKeepers.BLL.Validators.Communities;

public class UpdateCommunityValidator : AbstractValidator<UpdateCommunityCommand>
{
    public UpdateCommunityValidator()
    {
        RuleFor(x => x.RequestDto.Name)
            .NotEmpty().WithMessage(CommunityConstants.CommunityNameRequiredError)
            .MinimumLength(CommunityConstants.MinNameLenght).WithMessage(CommunityConstants.MinNameLenghtError)
            .MaximumLength(CommunityConstants.MaxNameLenght).WithMessage(CommunityConstants.MaxNameLenghtError);
    }
}
