using System.ComponentModel.DataAnnotations;
using RpgRooms.Core.Domain.Enums;

namespace RpgRooms.Core.Domain.Entities;

public class Campaign
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, StringLength(80, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public string OwnerUserId { get; set; } = string.Empty; // GM

    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;
    public bool IsRecruiting { get; set; } = false;
    public int MaxPlayers { get; set; } = 50; // regra fixa

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? FinalizedAt { get; set; }

    public ICollection<CampaignMember> Members { get; set; } = new List<CampaignMember>();
}
