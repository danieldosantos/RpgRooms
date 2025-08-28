namespace RpgRooms.Core.Domain.Entities;

public class AuditEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? CampaignId { get; set; }
    public string ActorUserId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty; // ex.: ToggleRecruitment, ApproveJoin, KickMember, FinalizeCampaign
    public string DataJson { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
