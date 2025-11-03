using AutoMapper;
using FluentValidation;
using FluentResults;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace KeyKeepers.BLL.Commands.Users.Update;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result<UserResponseDto>>
{
    private readonly IMapper mapper;
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IValidator<UpdateUserCommand> validator;
    private readonly IPasswordHasher<User> passwordHasher;

    public UpdateUserHandler(
        IMapper mapperObj,
        IRepositoryWrapper repositoryWrapperObj,
        IValidator<UpdateUserCommand> validatorObj,
        IPasswordHasher<User> passwordHasherObj)
    {
        mapper = mapperObj;
        repositoryWrapper = repositoryWrapperObj;
        validator = validatorObj;
        passwordHasher = passwordHasherObj;
    }

    public async Task<Result<UserResponseDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
        {
            return Result.Fail<UserResponseDto>(result.Errors.First().ErrorMessage);
        }

        QueryOptions<User> options = new QueryOptions<User>()
        {
            Filter = user => user.Id == request.RequestDto.UserId,
        };
        User? user = await repositoryWrapper.UserRepository.GetFirstOrDefaultAsync(options);

        mapper.Map(request.RequestDto, user);
        user!.PasswordHash = passwordHasher.HashPassword(user, request.RequestDto.Password);

        repositoryWrapper.UserRepository.Update(user);
        if (await repositoryWrapper.SaveChangesAsync() < 1)
        {
            return Result.Fail<UserResponseDto>(UserConstants.DbSaveError);
        }

        UserResponseDto response = mapper.Map<UserResponseDto>(user);

        return Result.Ok(response);
    }
}
