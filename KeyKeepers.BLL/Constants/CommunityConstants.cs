namespace KeyKeepers.BLL.Constants;

public static class CommunityConstants
{
    public static readonly int MaxNameLenght = 50;
    public static readonly int MinNameLenght = 3;

    public static readonly string OwnerIdRequiredError
        = "Owner ID is required!";

    public static readonly string CommunityNameRequiredError
        = "Community name is required!";

    public static readonly string MaxNameLenghtError
        = $"Community name must not exceed {MaxNameLenght} characters.";

    public static readonly string MinNameLenghtError
        = $"Community name must be longer than {MinNameLenght} characters!";

    public static readonly string AlreadyExistsError
        = "A community with this name already exists!";

    public static readonly string DbSaveError
        = "Database save error!";

    public static readonly string CommunityNotFoundError
        = "Community not found!";
}
