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

    public static readonly string NameRequiredErrorMessage = "Name is required";
    public static readonly string MaxNameLengthErrorMessage = $"Name must be less than {MaxNameLength} characters";
    public static readonly string MinNameLengthErrorMessage = $"Name must be more than {MinNameLength} characters";

    public static readonly string SurnameRequiredErrorMessage = "Surname is required";
    public static readonly string MaxSurnameLengthErrorMessage = $"Surname must be less than {MaxSurnameLength} characters";
    public static readonly string MinSurnameLengthErrorMessage = $"Surname must be more than {MinSurnameLength} characters";

    public static readonly string EmailRequiredErrorMessage = "Email is required";
    public static readonly string MaxEmailLengthErrorMessage = $"Email must be less than {MaxEmailLength} characters";
    public static readonly string MinEmailLengthErrorMessage = $"Email must be more than {MinEmailLength} characters";

    public static readonly string UserNameRequiredErrorMessage = "UserName is required";
    public static readonly string MaxUserNameErrorMessage = $"UserName must be less than {MaxUserNameLength} characters";
    public static readonly string MinUserNameErrorMessage = $"UserName must be more than {MinUserNameLength} characters";

    public static readonly string PasswordRequiredErrorMessage = "Password is required";
    public static readonly string PasswordLengthErrorMessage =
        $"Password must be less than {MinPasswordLength} characters";

    public static readonly string PasswordUppercaseLetterErrorMessage =
        "Password must contain at least one uppercase letter";

    public static readonly string PasswordDigitErrorMessage =
        "Password must contain at least one number";

    public static readonly string PasswordSpecialCharacterErrorMessage =
        "Password must contain at least one special character";

    public static readonly string UserCreationError = "User already exists";
    public static readonly string UserLogInError = "Invalid credentials!";
    public static readonly string UserLogOutError = "Invalid Token! Log out failed!";
}
