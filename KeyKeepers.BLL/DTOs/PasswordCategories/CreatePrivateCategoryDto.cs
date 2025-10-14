namespace KeyKeepers.BLL.DTOs.PasswordCategories;

public class CreatePrivateCategoryDto
{
    public long UserId { get; set; }

    public long? CommunityId { get; set; }

    public required string Name { get; set; }
}
