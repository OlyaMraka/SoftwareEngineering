using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Data;
using KeyKeepers.DAL.Repositories.Interfaces.PasswordCategories;
using KeyKeepers.DAL.Repositories.Realizations.Base;

namespace KeyKeepers.DAL.Repositories.Realizations.PasswordCategories;

public class PasswordCategoryRepository : RepositoryBase<PrivateCategory>, IPasswordCategoryRepository
{
    public PasswordCategoryRepository(KeyKeepersDbContext context)
        : base(context)
    {
    }
}
