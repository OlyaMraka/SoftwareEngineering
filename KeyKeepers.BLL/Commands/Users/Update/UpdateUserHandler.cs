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
    private readonly IValidator<string> passwordValidator;
    private readonly IPasswordHasher<User> passwordHasher;

    public UpdateUserHandler(
        IMapper mapperObj,
        IRepositoryWrapper repositoryWrapperObj,
        IValidator<UpdateUserCommand> validatorObj,
        IPasswordHasher<User> passwordHasherObj,
        IValidator<string> passwordValidatorObj)
    {
        mapper = mapperObj;
        repositoryWrapper = repositoryWrapperObj;
        validator = validatorObj;
        passwordHasher = passwordHasherObj;
        passwordValidator = passwordValidatorObj;
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
            AsNoTracking = false,
        };
        User? user = await repositoryWrapper.UserRepository.GetFirstOrDefaultAsync(options);

        if (user == null)
        {
            return Result.Fail<UserResponseDto>(UserConstants.UserNotFound);
        }

        mapper.Map(request.RequestDto, user);
        if (request.RequestDto.Password != string.Empty)
        {
            var passwordCheck = await passwordValidator.ValidateAsync(request.RequestDto.Password, cancellationToken);
            if (!passwordCheck.IsValid)
            {
                return Result.Fail<UserResponseDto>(passwordCheck.Errors.First().ErrorMessage);
            }

            user.PasswordHash = passwordHasher.HashPassword(user, request.RequestDto.Password);
        }

        repositoryWrapper.UserRepository.Update(user);
        if (await repositoryWrapper.SaveChangesAsync() < 1)
        {
            return Result.Fail<UserResponseDto>(UserConstants.DbSaveError);
        }

        UserResponseDto response = mapper.Map<UserResponseDto>(user);

        return Result.Ok(response);
    }
}
