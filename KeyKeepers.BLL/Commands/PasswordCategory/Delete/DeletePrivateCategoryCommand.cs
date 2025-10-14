using FluentResults;
using MediatR;

namespace KeyKeepers.BLL.Commands.PasswordCategory.Delete;

public record DeletePrivateCategoryCommand(long Id) : IRequest<Result<long>>;
