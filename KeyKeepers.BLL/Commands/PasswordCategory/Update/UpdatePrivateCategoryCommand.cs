using MediatR;
using FluentResults;
using KeyKeepers.BLL.DTOs.PasswordCategories;

namespace KeyKeepers.BLL.Commands.PasswordCategory.Update;

public record UpdatePrivateCategoryCommand(UpdatePrivateCategoryDto RequestDto)
    : IRequest<Result<PrivateCategoryResponseDto>>;
