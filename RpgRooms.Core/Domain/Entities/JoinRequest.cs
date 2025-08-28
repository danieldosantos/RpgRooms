using RpgRooms.Core.Domain.Enums;

namespace RpgRooms.Core.Domain.Entities;

public class JoinRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CampaignId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Message { get; set; }
    public JoinRequestStatus Status { get; set; } = JoinRequestStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DecisionAt { get; set; }
}
