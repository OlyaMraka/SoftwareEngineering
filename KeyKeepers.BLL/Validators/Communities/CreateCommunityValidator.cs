using FluentValidation;
using KeyKeepers.BLL.Commands.Communities.Create;
using KeyKeepers.BLL.Constants;

namespace KeyKeepers.BLL.Validators.Communities;

public class CreateCommunityValidator : AbstractValidator<CreateCommunityCommand>
{
    public CreateCommunityValidator()
    {
        RuleFor(x => x.RequestDto.OwnerId)
            .NotEmpty().WithMessage(CommunityConstants.OwnerIdRequiredError);

        RuleFor(x => x.RequestDto.Name)
            .NotEmpty().WithMessage(CommunityConstants.CommunityNameRequiredError)
            .MinimumLength(CommunityConstants.MinNameLenght).WithMessage(CommunityConstants.MinNameLenghtError)
            .MaximumLength(CommunityConstants.MaxNameLenght).WithMessage(CommunityConstants.MaxNameLenghtError);
    }
}
