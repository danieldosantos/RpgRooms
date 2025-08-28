namespace RpgRooms.Core.Domain.Entities;

public class CampaignMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CampaignId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? CharacterName { get; set; }
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsBanned { get; set; } = false;
}
