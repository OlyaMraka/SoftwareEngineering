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
        = "First name is required!";

    public static readonly string MaxNameLengthErrorMessage
        = $"First name must be shorter than {MaxNameLength} characters!";

    public static readonly string MinNameLengthErrorMessage
        = $"First name must be longer than {MinNameLength} characters!";

    public static readonly string SurnameRequiredErrorMessage
        = "Last name is required!";

    public static readonly string MaxSurnameLengthErrorMessage
        = $"Last name must be shorter than {MaxSurnameLength} characters!";

    public static readonly string MinSurnameLengthErrorMessage
        = $"Last name must be longer than {MinSurnameLength} characters!";

    public static readonly string EmailRequiredErrorMessage
        = "Email is required!";

    public static readonly string MaxEmailLengthErrorMessage
        = $"Email must be shorter than {MaxEmailLength} characters!";

    public static readonly string MinEmailLengthErrorMessage
        = $"Email must be longer than {MinEmailLength} characters!";

    public static readonly string UserNameRequiredErrorMessage
        = "Username is required!";

    public static readonly string MaxUserNameErrorMessage
        = $"Username must be shorter than {MaxUserNameLength} characters!";

    public static readonly string MinUserNameErrorMessage
        = $"Username must be longer than {MinUserNameLength} characters!";

    public static readonly string PasswordRequiredErrorMessage
        = "Password is required!";

    public static readonly string PasswordLengthErrorMessage =
        $"Password must be longer than {MinPasswordLength} characters!";

    public static readonly string PasswordUppercaseLetterErrorMessage =
        "Password must contain at least one uppercase letter!";

    public static readonly string PasswordDigitErrorMessage =
        "Password must contain at least one digit!";

    public static readonly string PasswordSpecialCharacterErrorMessage =
        "Password must contain special characters!";

    public static readonly string UserCreationError
        = "A user with this information already exists!";

    public static readonly string UserLogInError
        = "Incorrect login or password!";

    public static readonly string UserLogOutError
        = "Invalid token!";

    public static readonly string UserNotFound
        = "User not found!";

    public static readonly string DbSaveError
        = "Database save error!";

    public static readonly string DataNotFound
        = "Data not found!";
}
