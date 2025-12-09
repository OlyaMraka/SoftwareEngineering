namespace KeyKeepers.BLL.Constants;

public static class PasswordConstants
{
    public static readonly int MinAppNameLenght = 3;
    public static readonly int MaxAppNameLenght = 30;
    public static readonly int MaxPasswordLenght = 30;
    public static readonly int MaxLoginLenght = 50;

    public static readonly string SaveDataBaseError
        = "Error saving data!";

    public static readonly string MinAppNameLenghtError
        = $"Minimum application name length is {MinAppNameLenght} characters!";

    public static readonly string MaxAppNameLenghtError
        = $"Maximum application name length is {MaxAppNameLenght} characters!";

    public static readonly string MaxPasswordLenghtError
        = $"Maximum password length is {MaxPasswordLenght} characters!";

    public static readonly string MaxLoginLenghtError
        = $"Maximum login length is {MaxLoginLenght} characters!";

    public static readonly string PasswordRequiredError
        = "Password is required!";

    public static readonly string LoginRequiredError
        = "Login is required!";

    public static readonly string AppNameRequiredError
        = "Application name is required!";

    public static readonly string AlreadyExistsError
        = "You already have a record with this application and login!";

    public static readonly string NotFoundError
        = "Record not found!";
}
