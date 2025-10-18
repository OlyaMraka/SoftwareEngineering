namespace KeyKeepers.BLL.Constants;

public static class PasswordConstants
{
    public static readonly int MinAppNameLenght = 3;
    public static readonly int MaxAppNameLenght = 30;
    public static readonly int MaxPasswordLenght = 30;
    public static readonly int MaxLoginLenght = 50;

    public static readonly string SaveDataBaseError
        = "Помилка збереження даних!";

    public static readonly string MinAppNameLenghtError
        = $"Мінімальна довжина назви додатку {MinAppNameLenght} символів!";

    public static readonly string MaxAppNameLenghtError
        = $"Максимальна довжина назви додатку {MaxAppNameLenght} символів!";

    public static readonly string MaxPasswordLenghtError
        = $"Максимальна довжина паролю {MaxPasswordLenght} символів!";

    public static readonly string MaxLoginLenghtError
        = $"Максимальна довжина логіну {MaxLoginLenght} символів!";

    public static readonly string PasswordRequiredError
        = "Пароль обов'язковай!";

    public static readonly string LoginRequiredError
        = "Логін обов'язковий!";

    public static readonly string AppNameRequiredError
        = "Ім'я додатку обов'язкове!";

    public static readonly string AlreadyExistsError
        = "У вас уже існує запис з таким додатком і логіном!";

    public static readonly string NotFoundError
        = "Такого запису не знайдено!";
}
