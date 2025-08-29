namespace RpgRooms.Core.Domain.Entities;

public class CampaignExit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CampaignId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTimeOffset ExitedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsKick { get; set; } = false;
}
