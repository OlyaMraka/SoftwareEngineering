using MediatR;
using FluentResults;
using KeyKeepers.BLL.DTOs.PasswordCategories;

namespace KeyKeepers.BLL.Queries.PasswordCategories.GetAll;

public record GetAllPasswordCategoriesQuery(long UserId)
    : IRequest<Result<IEnumerable<PrivateCategoryResponseDto>>>;
