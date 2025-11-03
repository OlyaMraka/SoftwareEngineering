namespace KeyKeepers.BLL.Constants;

public static class CommunityConstants
{
    public static readonly int MaxNameLenght = 30;
    public static readonly int MinNameLenght = 3;

    public static readonly string MaxNameLenghtError
        = $"Назва спільноти не повинна перевищувати {MaxNameLenght} символів";

    public static readonly string MinNameLenghtError
        = $"Назва повинна перевищувати {MinNameLenght} символів!";

    public static readonly string AlreadyExistsError
        = "У вас вже існує категорія з даним ім'ям!";

    public static readonly string DbSaveError
        = "Помилка збереження бази даних!";
}
