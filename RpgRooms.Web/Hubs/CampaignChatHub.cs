using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RpgRooms.Core.Application.Interfaces;
using RpgRooms.Core.Application.DTOs;

namespace RpgRooms.Web.Hubs;

[Authorize]
public class CampaignChatHub : Hub
{
    private readonly ICampaignService _svc;
    public CampaignChatHub(ICampaignService svc) => _svc = svc;

    private static string GroupName(Guid campaignId) => $"campaign-{campaignId}";

    public async Task JoinCampaignGroup(Guid campaignId)
    {
        var userId = Context.User!.Identity!.Name!;
        if (!await _svc.IsMemberAsync(campaignId, userId) && !await _svc.IsGmAsync(campaignId, userId))
            throw new HubException("Acesso negado ao chat desta campanha.");
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(campaignId));
        await Clients.Group(GroupName(campaignId)).SendAsync("SystemNotice", $"{userId} entrou no chat.");
    }

    public async Task SendMessage(Guid campaignId, string displayName, string content, bool sentAsCharacter)
    {
        var userId = Context.User!.Identity!.Name!;
        var msg = await _svc.AddChatMessageAsync(campaignId, userId, displayName, content, sentAsCharacter);
        var dto = new ChatMessageDto(msg.Id, msg.DisplayName, msg.Content, msg.SentAsCharacter, msg.CreatedAt);
        await Clients.Group(GroupName(campaignId)).SendAsync("ReceiveMessage", dto);
    }
}
