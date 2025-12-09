namespace KeyKeepers.BLL.Constants;

public class PasswordCategoriesConstants
{
    public static readonly int MaxNameLength = 20;
    public static readonly int MinNameLength = 2;

    public static readonly string NameRequiredErrorMessage
        = "Category name is required!";

    public static readonly string MaxNameLengthErrorMessage
        = $"Name must be shorter than {MaxNameLength} characters!";

    public static readonly string MinNameLengthErrorMessage
        = $"Name must be longer than {MinNameLength} characters!";

    public static readonly string CategoryAlreadyExistsErrorMessage
        = "A private category with this name already exists!";

    public static readonly string DbSaveErrorMessage
        = "Database save error!";

    public static readonly string ErrorMessage
        = "An error occurred while creating the category.";

    public static readonly string CategoryNotFound
        = "Category not found!";

    public static readonly string ImpossibleToDelete
        = "Impossible to delete the category because it contains credentials!";
}
