using MediatR;
using FluentResults;
using KeyKeepers.BLL.DTOs.PasswordCategories;

namespace KeyKeepers.BLL.Commands.PasswordCategory.Create;

public record CreatePrivateCategoryCommand(CreatePrivateCategoryDto RequestDto)
    : IRequest<Result<PrivateCategoryResponseDto>>;
