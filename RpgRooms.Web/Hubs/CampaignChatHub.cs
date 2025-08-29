using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RpgRooms.Core.Application.Interfaces;
using RpgRooms.Core.Application.DTOs;
using System.Collections.Concurrent;
using System.Linq;

namespace RpgRooms.Web.Hubs;

[Authorize]
public class CampaignChatHub : Hub
{
    private readonly ICampaignService _svc;
    public CampaignChatHub(ICampaignService svc) => _svc = svc;

    private static readonly ConcurrentDictionary<Guid, HashSet<string>> _connectedUsers = new();
    private static readonly ConcurrentDictionary<string, (Guid CampaignId, string UserId)> _connections = new();

    private static string GroupName(Guid campaignId) => $"campaign-{campaignId}";

    public async Task JoinCampaignGroup(Guid campaignId)
    {
        var userId = Context.User!.Identity!.Name!;
        if (!await _svc.IsMemberAsync(campaignId, userId) && !await _svc.IsGmAsync(campaignId, userId))
            throw new HubException("Acesso negado ao chat desta campanha.");
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(campaignId));

        _connections[Context.ConnectionId] = (campaignId, userId);

        var users = _connectedUsers.GetOrAdd(campaignId, _ => new HashSet<string>());
        bool isFirst;
        lock (users)
        {
            isFirst = users.Add(userId);
        }

        if (isFirst)
            await Clients.Group(GroupName(campaignId)).SendAsync("SystemNotice", $"{userId} entrou no chat.");
    }

    public async Task SendMessage(Guid campaignId, string displayName, string content, bool sentAsCharacter)
    {
        var userId = Context.User!.Identity!.Name!;
        var msg = await _svc.AddChatMessageAsync(campaignId, userId, displayName, content, sentAsCharacter);
        var dto = new ChatMessageDto(msg.Id, msg.DisplayName, msg.Content, msg.SentAsCharacter);
        await Clients.Group(GroupName(campaignId)).SendAsync("ReceiveMessage", dto);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connections.TryRemove(Context.ConnectionId, out var info))
        {
            var (campaignId, userId) = info;
            var remaining = _connections.Any(c => c.Value.CampaignId == campaignId && c.Value.UserId == userId);

            if (!remaining && _connectedUsers.TryGetValue(campaignId, out var users))
            {
                bool removed;
                lock (users)
                {
                    removed = users.Remove(userId);
                    if (users.Count == 0)
                        _connectedUsers.TryRemove(campaignId, out _);
                }

                if (removed)
                    await Clients.Group(GroupName(campaignId)).SendAsync("SystemNotice", $"{userId} saiu do chat.");
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
