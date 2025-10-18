using FluentResults;
using MediatR;
using KeyKeepers.BLL.DTOs.Passwords;

namespace KeyKeepers.BLL.Queries.Passwords.GetAllById;

public record GetCredentialsByIdQuery(long CategoryId)
    : IRequest<Result<IEnumerable<PasswordResponse>>>;
