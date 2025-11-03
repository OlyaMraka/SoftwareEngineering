namespace KeyKeepers.BLL.Constants;

public static class UserConstants
{
    public static readonly int MaxNameLength = 30;
    public static readonly int MinNameLength = 3;
    public static readonly int MaxSurnameLength = 30;
    public static readonly int MinSurnameLength = 3;
    public static readonly int MaxEmailLength = 40;
    public static readonly int MinEmailLength = 4;
    public static readonly int MaxUserNameLength = 40;
    public static readonly int MinUserNameLength = 4;
    public static readonly int MinPasswordLength = 8;

    public static readonly string NameRequiredErrorMessage
        = "Ім'я обов'язкове!";

    public static readonly string MaxNameLengthErrorMessage
        = $"Ім'я повинне бути менше ніж {MaxNameLength} символів!";

    public static readonly string MinNameLengthErrorMessage
        = $"Ім'я повинне бути більше ніж {MinNameLength} символів!";

    public static readonly string SurnameRequiredErrorMessage
        = "Прізвище обов'язкове!";

    public static readonly string MaxSurnameLengthErrorMessage
        = $"Прізвище повинне бути менше ніж {MaxSurnameLength} символів!";

    public static readonly string MinSurnameLengthErrorMessage
        = $"Прізвище повинне бути більше ніж {MinSurnameLength} символів!";

    public static readonly string EmailRequiredErrorMessage
        = "Email обов'язковий!";

    public static readonly string MaxEmailLengthErrorMessage
        = $"Email повинний бути менше ніж {MaxEmailLength} символів!";

    public static readonly string MinEmailLengthErrorMessage
        = $"Email повинний бути більше ніж {MinEmailLength} символів!";

    public static readonly string UserNameRequiredErrorMessage
        = "Ім'я користувача обов'язкове!";

    public static readonly string MaxUserNameErrorMessage
        = $"Ім'я користувача повинне бути менше ніж {MaxUserNameLength} символів!";

    public static readonly string MinUserNameErrorMessage
        = $"Ім'я користувача повинне бути більше ніж {MinUserNameLength} символів!";

    public static readonly string PasswordRequiredErrorMessage
        = "Пароль обов'язковий!";

    public static readonly string PasswordLengthErrorMessage =
        $"Пароль повинний бути більше ніж {MinPasswordLength} символів!";

    public static readonly string PasswordUppercaseLetterErrorMessage =
        "Пароль повинен містити хоч одну велику літеру!";

    public static readonly string PasswordDigitErrorMessage =
        "Пароль повинен містити хоч одну цифру!";

    public static readonly string PasswordSpecialCharacterErrorMessage =
        "Пароль має містити спецсимволи!";

    public static readonly string UserCreationError
        = "Такий користувач уже існує!";

    public static readonly string UserLogInError
        = "Не правильний пароль або логін!";

    public static readonly string UserLogOutError
        = "Невалідний токен!";

    public static readonly string UserNotFound
        = "Користувач не знайдений!";

    public static readonly string DbSaveError
        = "Помилка збереження бази даних!";
}
