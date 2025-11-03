namespace KeyKeepers.BLL.DTOs.Communities;

public class CreateCommunityDto
{
    public long OwnerId { get; set; }

    public string Name { get; set; } = string.Empty;
}
