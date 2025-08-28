namespace RpgRooms.Core.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CampaignId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty; // real ou personagem no momento do envio
    public string Content { get; set; } = string.Empty;     // máx 1000
    public bool SentAsCharacter { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
