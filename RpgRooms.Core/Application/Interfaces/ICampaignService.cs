using RpgRooms.Core.Domain.Entities;

namespace RpgRooms.Core.Application.Interfaces;

public interface ICampaignService
{
    Task<Campaign> CreateCampaignAsync(string ownerUserId, string name, string? description);
    Task<bool> ToggleRecruitmentAsync(Guid campaignId, string gmUserId);
    Task FinalizeCampaignAsync(Guid campaignId, string gmUserId);

    Task<JoinRequest> CreateJoinRequestAsync(Guid campaignId, string userId, string? message);
    Task ApproveJoinRequestAsync(Guid campaignId, Guid requestId, string gmUserId);
    Task RejectJoinRequestAsync(Guid campaignId, Guid requestId, string gmUserId);

    Task RemoveMemberAsync(Guid campaignId, string targetUserId, string gmUserId, string? reason = null);

    Task<IReadOnlyList<Campaign>> ListCampaignsAsync(string? search, bool recruitingOnly, string? ownerUserId, string? status);
    Task<Campaign?> GetCampaignAsync(Guid campaignId);

    Task<ChatMessage> AddChatMessageAsync(Guid campaignId, string userId, string displayName, string content, bool sentAsCharacter);
    Task<bool> IsMemberAsync(Guid campaignId, string userId);
    Task<bool> IsGmAsync(Guid campaignId, string userId);
    Task<int> CountMembersAsync(Guid campaignId);
}
