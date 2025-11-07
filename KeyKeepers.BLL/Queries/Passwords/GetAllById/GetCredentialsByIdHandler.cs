using AutoMapper;
using KeyKeepers.BLL.DTOs.Passwords;
using MediatR;
using FluentResults;
using KeyKeepers.BLL.Interfaces;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;

namespace KeyKeepers.BLL.Queries.Passwords.GetAllById;

public class GetCredentialsByIdHandler : IRequestHandler<GetCredentialsByIdQuery, Result<IEnumerable<PasswordResponse>>>
{
    private readonly IMapper mapper;
    private readonly IEncryptionService encryptionService;
    private readonly IRepositoryWrapper repositoryWrapper;

    public GetCredentialsByIdHandler(
        IMapper mapperObj,
        IRepositoryWrapper repositoryWrapperObj,
        IEncryptionService encryptionServiceObj)
    {
        mapper = mapperObj;
        encryptionService = encryptionServiceObj;
        repositoryWrapper = repositoryWrapperObj;
    }

    public async Task<Result<IEnumerable<PasswordResponse>>> Handle(
        GetCredentialsByIdQuery request,
        CancellationToken cancellationToken)
    {
        var options = new QueryOptions<Credentials>
        {
            Filter = x => x.CategoryId == request.CategoryId,
        };

        var entities = await repositoryWrapper.PasswordRepository.GetAllAsync(options);

        foreach (var entity in entities)
        {
            entity.PasswordHash = encryptionService.Decrypt(entity.PasswordHash);
        }

        return Result.Ok(mapper.Map<IEnumerable<PasswordResponse>>(entities));
    }
}
