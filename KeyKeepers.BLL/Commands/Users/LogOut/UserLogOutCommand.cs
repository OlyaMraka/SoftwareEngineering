using FluentResults;
using KeyKeepers.BLL.DTOs.Users;
using MediatR;

namespace KeyKeepers.BLL.Commands.Users.LogOut;

public record UserLogOutCommand(UserLogOutDto UserLogOutDto) : IRequest<Result>;
