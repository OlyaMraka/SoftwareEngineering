using KeyKeepers.DAL.Enums;

namespace KeyKeepers.BLL.DTOs.JoinRequests;

public class AcceptOrDeclineDto
{
    public long Id { get; set; }

    public RequestStatus Status { get; set; }
}
