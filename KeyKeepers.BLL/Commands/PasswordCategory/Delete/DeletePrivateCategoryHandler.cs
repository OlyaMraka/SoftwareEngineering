using MediatR;
using FluentResults;
using KeyKeepers.BLL.Constants;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Microsoft.EntityFrameworkCore;

namespace KeyKeepers.BLL.Commands.PasswordCategory.Delete;

public class DeletePrivateCategoryHandler : IRequestHandler<DeletePrivateCategoryCommand, Result<long>>
{
    private readonly IRepositoryWrapper repositoryWrapper;

    public DeletePrivateCategoryHandler(IRepositoryWrapper repositoryWrapperObj)
    {
        repositoryWrapper = repositoryWrapperObj;
    }

    public async Task<Result<long>> Handle(DeletePrivateCategoryCommand request, CancellationToken cancellationToken)
    {
        PrivateCategory? entity = await repositoryWrapper
            .PrivatePasswordCategoryRepository
            .GetFirstOrDefaultAsync(new QueryOptions<PrivateCategory>
            {
                Filter = category => category.Id == request.Id,
                Include = category => category
                    .Include(c => c.CredentialsCollection),
            });

        if (entity == null)
        {
            return Result.Fail(PasswordCategoriesConstants.CategoryNotFound);
        }

        if (entity.CredentialsCollection.Count != 0)
        {
            return Result.Fail(PasswordCategoriesConstants.ImpossibleToDelete);
        }

        repositoryWrapper.PrivatePasswordCategoryRepository.Delete(entity);

        if (await repositoryWrapper.SaveChangesAsync() <= 0)
        {
            return Result.Fail(PasswordCategoriesConstants.DbSaveErrorMessage);
        }

        return entity.Id;
    }
}
