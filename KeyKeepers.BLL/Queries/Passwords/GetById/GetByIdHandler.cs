using AutoMapper;
using MediatR;
using FluentResults;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Passwords;
using KeyKeepers.BLL.Interfaces;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;

namespace KeyKeepers.BLL.Queries.Passwords.GetById;

public class GetByIdHandler : IRequestHandler<GetByIdQuery, Result<PasswordResponse>>
{
    private readonly IMapper mapper;
    private readonly IEncryptionService encryptionService;
    private readonly IRepositoryWrapper repositoryWrapper;

    public GetByIdHandler(
        IMapper mapperObj,
        IRepositoryWrapper repositoryWrapperObj,
        IEncryptionService encryptionServiceObj)
    {
        mapper = mapperObj;
        encryptionService = encryptionServiceObj;
        repositoryWrapper = repositoryWrapperObj;
    }

    public async Task<Result<PasswordResponse>> Handle(GetByIdQuery request, CancellationToken cancellationToken)
    {
        var options = new QueryOptions<Credentials>
        {
            Filter = password => password.Id == request.Id,
        };

        var password = await repositoryWrapper.PasswordRepository.GetFirstOrDefaultAsync(options);

        if (password == null)
        {
            return Result.Fail<PasswordResponse>(PasswordConstants.NotFoundError);
        }

        var response = mapper.Map<PasswordResponse>(password);
        response.Password = encryptionService.Decrypt(response.Password);

        return Result.Ok(response);
    }
}
