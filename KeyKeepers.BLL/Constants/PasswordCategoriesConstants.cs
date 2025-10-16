namespace KeyKeepers.BLL.Constants;

public class PasswordCategoriesConstants
{
    public static readonly int MaxNameLength = 20;
    public static readonly int MinNameLength = 2;

    public static readonly string NameRequiredErrorMessage
        = "Ім'я категорії обов'язкове!";

    public static readonly string MaxNameLengthErrorMessage
        = $"Ім'я повинне бути менше ніж {MaxNameLength} симовлів!";

    public static readonly string MinNameLengthErrorMessage
        = $"Ім'я повинне бути більше ніж {MinNameLength} символів!";

    public static readonly string CategoryAlreadyExistsErrorMessage
        = "Приватна категорія з даним ім'ям уже існує!";

    public static readonly string DbSaveErrorMessage
        = "Помилка збереження бази даних!";

    public static readonly string ErrorMessage
        = "Виникла помилка при створенні категорії";

    public static readonly string CategoryNotFound
        = "Категорія не знайдена!";

    public static readonly string ImpossibleToDelete
        = "Неможливо видалити категорію оскільки вона містить записи!";
}
