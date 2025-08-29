using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RpgRooms.Core.Application.Interfaces;
using RpgRooms.Core.Domain.Entities;
using RpgRooms.Core.Domain.Enums;
using RpgRooms.Infrastructure.Data;

namespace RpgRooms.Infrastructure.Services;

public class CampaignService : ICampaignService
{
    private readonly AppDbContext _db;
    public const int MAX_PLAYERS = 50;

    public CampaignService(AppDbContext db) => _db = db;

    public async Task<Campaign> CreateCampaignAsync(string ownerUserId, string name, string? description)
    {
        var existing = await _db.Campaigns
            .FirstOrDefaultAsync(c => c.OwnerUserId == ownerUserId && c.Name == name);
        if (existing != null)
            return existing;

        var c = new Campaign
        {
            OwnerUserId = ownerUserId,
            Name = name,
            Description = description,
            Status = CampaignStatus.InProgress,
            IsRecruiting = false,
            MaxPlayers = MAX_PLAYERS
        };
        _db.Campaigns.Add(c);
        await _db.SaveChangesAsync();
        await Audit("CreateCampaign", ownerUserId, c.Id, new { name });
        return c;
    }

    public async Task<bool> ToggleRecruitmentAsync(Guid campaignId, string gmUserId)
    {
        var c = await _db.Campaigns.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == campaignId)
            ?? throw new InvalidOperationException("Campanha não encontrada");
        if (c.OwnerUserId != gmUserId) throw new UnauthorizedAccessException("Apenas o GM pode alterar o recrutamento.");
        if (c.Status == CampaignStatus.Finalized) throw new InvalidOperationException("Campanha finalizada.");

        var count = c.Members.Count(m => !m.IsBanned);
        if (count >= MAX_PLAYERS && !c.IsRecruiting)
            throw new InvalidOperationException("Campanha lotada (50/50).");

        c.IsRecruiting = !c.IsRecruiting;
        if (c.IsRecruiting && c.Status == CampaignStatus.InProgress)
            c.Status = CampaignStatus.Recruiting;
        if (!c.IsRecruiting && c.Status == CampaignStatus.Recruiting && count > 0)
            c.Status = CampaignStatus.InProgress;

        c.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        await Audit("ToggleRecruitment", gmUserId, c.Id, new { c.IsRecruiting });
        return c.IsRecruiting;
    }

    public async Task FinalizeCampaignAsync(Guid campaignId, string gmUserId)
    {
        var c = await _db.Campaigns.FindAsync(campaignId) ?? throw new InvalidOperationException("Campanha não encontrada");
        if (c.OwnerUserId != gmUserId) throw new UnauthorizedAccessException("Apenas o GM pode finalizar.");

        c.Status = CampaignStatus.Finalized;
        c.IsRecruiting = false;
        c.FinalizedAt = DateTimeOffset.UtcNow;
        c.UpdatedAt = DateTimeOffset.UtcNow;

        var members = await _db.CampaignMembers.Where(m => m.CampaignId == campaignId).ToListAsync();
        if (members.Any())
            _db.CampaignMembers.RemoveRange(members);

        c.OwnerUserId = null;
        await _db.SaveChangesAsync();

        foreach (var m in members)
            await Audit("FinalizeRemoveMember", gmUserId, c.Id, new { m.UserId });

        await Audit("FinalizeCampaign", gmUserId, c.Id, new { c.Status });
    }

    public async Task<JoinRequest> CreateJoinRequestAsync(Guid campaignId, string userId, string? message)
    {
        var c = await _db.Campaigns.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == campaignId)
            ?? throw new InvalidOperationException("Campanha não encontrada");
        if (c.Status == CampaignStatus.Finalized) throw new InvalidOperationException("Campanha finalizada.");
        if (!c.IsRecruiting) throw new InvalidOperationException("Campanha não está recrutando.");
        if (c.Members.Count(m => !m.IsBanned) >= MAX_PLAYERS) throw new InvalidOperationException("Campanha lotada (50/50).");
        if (await _db.JoinRequests.AnyAsync(r => r.CampaignId == campaignId && r.UserId == userId && r.Status == JoinRequestStatus.Pending))
            throw new InvalidOperationException("Já existe uma solicitação pendente.");
        if (await _db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId))
            throw new InvalidOperationException("Você já é membro.");

        var cutoff = DateTimeOffset.UtcNow.AddHours(-12);
        var recentExit = await _db.CampaignExits
            .AnyAsync(e => e.CampaignId == campaignId && e.UserId == userId && e.ExitedAt > cutoff);
        if (recentExit)
            throw new InvalidOperationException("Aguarde 12h antes de solicitar novamente.");

        var req = new JoinRequest { CampaignId = campaignId, UserId = userId, Message = message };
        _db.JoinRequests.Add(req);
        await _db.SaveChangesAsync();
        await Audit("CreateJoinRequest", userId, campaignId, new { message });
        return req;
    }

    public async Task ApproveJoinRequestAsync(Guid campaignId, Guid requestId, string gmUserId)
    {
        var c = await _db.Campaigns.Include(x => x.Members).FirstAsync(x => x.Id == campaignId);
        if (c.OwnerUserId != gmUserId) throw new UnauthorizedAccessException("Apenas o GM pode aprovar.");
        var req = await _db.JoinRequests.FirstAsync(r => r.Id == requestId && r.CampaignId == campaignId);
        if (req.Status != JoinRequestStatus.Pending) throw new InvalidOperationException("Solicitação não está pendente.");
        if (c.Members.Count(m => !m.IsBanned) >= MAX_PLAYERS) throw new InvalidOperationException("Campanha lotada.");

        req.Status = JoinRequestStatus.Approved;
        req.DecisionAt = DateTimeOffset.UtcNow;
        _db.CampaignMembers.Add(new CampaignMember { CampaignId = campaignId, UserId = req.UserId });
        _db.Characters.Add(new Character
        {
            CampaignId = campaignId,
            UserId = req.UserId,
            Name = "New Character",
            Level = 1,
            Str = 10,
            Dex = 10,
            Con = 10,
            Int = 10,
            Wis = 10,
            Cha = 10
        });
        await _db.SaveChangesAsync();
        await Audit("ApproveJoin", gmUserId, campaignId, new { req.UserId });

        var count = await _db.CampaignMembers.CountAsync(m => m.CampaignId == campaignId && !m.IsBanned);
        if (count >= MAX_PLAYERS)
        {
            c.IsRecruiting = false; // fecha automático
            if (c.Status == CampaignStatus.Recruiting) c.Status = CampaignStatus.InProgress;
            await _db.SaveChangesAsync();
            await Audit("AutoCloseRecruitmentAt50", gmUserId, campaignId, new { count });
        }
    }

    public async Task RejectJoinRequestAsync(Guid campaignId, Guid requestId, string gmUserId)
    {
        var c = await _db.Campaigns.FindAsync(campaignId) ?? throw new InvalidOperationException("Campanha não encontrada");
        if (c.OwnerUserId != gmUserId) throw new UnauthorizedAccessException("Apenas o GM pode rejeitar.");
        var req = await _db.JoinRequests.FirstAsync(r => r.Id == requestId && r.CampaignId == campaignId);
        if (req.Status != JoinRequestStatus.Pending) throw new InvalidOperationException("Solicitação não está pendente.");
        req.Status = JoinRequestStatus.Rejected;
        req.DecisionAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        await Audit("RejectJoin", gmUserId, campaignId, new { req.UserId });
    }

    public async Task<IReadOnlyList<JoinRequest>> ListJoinRequestsAsync(Guid campaignId, string gmUserId)
    {
        var c = await _db.Campaigns.FindAsync(campaignId) ?? throw new InvalidOperationException("Campanha não encontrada");
        if (c.OwnerUserId != gmUserId) throw new UnauthorizedAccessException("Apenas o GM pode ver solicitações.");
        var list = await _db.JoinRequests
            .Where(r => r.CampaignId == campaignId && r.Status == JoinRequestStatus.Pending)
            .ToListAsync();
        return list.OrderBy(r => r.CreatedAt).ToList();
    }

    public async Task<IReadOnlyList<CampaignMember>> ListMembersAsync(Guid campaignId, string gmUserId)
    {
        var c = await _db.Campaigns.FindAsync(campaignId) ?? throw new InvalidOperationException("Campanha não encontrada");
        if (c.OwnerUserId != gmUserId) throw new UnauthorizedAccessException("Apenas o GM pode ver membros.");
        var list = await _db.CampaignMembers
            .Where(m => m.CampaignId == campaignId && !m.IsBanned)
            .ToListAsync();
        return list.OrderBy(m => m.JoinedAt).ToList();
    }

    public async Task RemoveMemberAsync(Guid campaignId, string targetUserId, string gmUserId, string? reason = null)
    {
        var c = await _db.Campaigns.FirstAsync(x => x.Id == campaignId);
        if (c.OwnerUserId != gmUserId) throw new UnauthorizedAccessException("Apenas o GM pode remover jogadores.");
        var member = await _db.CampaignMembers.FirstOrDefaultAsync(m => m.CampaignId == campaignId && m.UserId == targetUserId)
            ?? throw new InvalidOperationException("Membro não encontrado");
        _db.CampaignMembers.Remove(member);
        _db.CampaignExits.Add(new CampaignExit { CampaignId = campaignId, UserId = targetUserId, IsKick = true });
        await _db.SaveChangesAsync();
        await Audit("KickMember", gmUserId, campaignId, new { targetUserId, reason });
    }

    public async Task LeaveCampaignAsync(Guid campaignId, string userId)
    {
        var c = await _db.Campaigns.FirstOrDefaultAsync(x => x.Id == campaignId)
            ?? throw new InvalidOperationException("Campanha não encontrada");
        if (c.OwnerUserId == userId) throw new InvalidOperationException("GM não pode sair da própria campanha.");
        var member = await _db.CampaignMembers.FirstOrDefaultAsync(m => m.CampaignId == campaignId && m.UserId == userId)
            ?? throw new InvalidOperationException("Você não é membro desta campanha");

        _db.CampaignMembers.Remove(member);
        _db.CampaignExits.Add(new CampaignExit { CampaignId = campaignId, UserId = userId, IsKick = false });
        await _db.SaveChangesAsync();
        await Audit("LeaveCampaign", userId, campaignId, new { });

        var count = await _db.CampaignMembers.CountAsync(m => m.CampaignId == campaignId && !m.IsBanned);
        if (!c.IsRecruiting && c.Status != CampaignStatus.Finalized && count < MAX_PLAYERS)
        {
            c.IsRecruiting = true;
            if (c.Status == CampaignStatus.InProgress) c.Status = CampaignStatus.Recruiting;
            c.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            await Audit("AutoOpenRecruitmentOnLeave", userId, campaignId, new { count });
        }
    }

    public async Task<IReadOnlyList<Campaign>> ListUserCampaignsAsync(string userId)
    {
        var list = await _db.Campaigns
            .Include(c => c.Members)
            .Where(c => c.OwnerUserId == userId || c.Members.Any(m => m.UserId == userId && !m.IsBanned))
            .ToListAsync();
        return list.OrderByDescending(c => c.CreatedAt).ToList();
    }

    public async Task<IReadOnlyList<Campaign>> ListCampaignsAsync(string? search, bool recruitingOnly, string? ownerUserId, string? status)
    {
        var q = _db.Campaigns.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(c => c.Name.Contains(search) || (c.Description ?? "").Contains(search));
        if (recruitingOnly)
            q = q.Where(c => c.IsRecruiting);
        else
            q = q.Where(c => c.IsRecruiting || c.Status == CampaignStatus.Finalized);
        if (!string.IsNullOrWhiteSpace(ownerUserId)) q = q.Where(c => c.OwnerUserId == ownerUserId);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CampaignStatus>(status, out var st)) q = q.Where(c => c.Status == st);

        // SQLite does not support ordering by DateTimeOffset, so we order in-memory
        var list = await q.ToListAsync();
        return list.OrderByDescending(c => c.CreatedAt).ToList();
    }

    public Task<Campaign?> GetCampaignAsync(Guid campaignId) => _db.Campaigns.FirstOrDefaultAsync(c => c.Id == campaignId);

    public async Task<ChatMessage> AddChatMessageAsync(Guid campaignId, string userId, string displayName, string content, bool sentAsCharacter)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length > 1000)
            throw new InvalidOperationException("Mensagem inválida.");
        var c = await _db.Campaigns.FindAsync(campaignId) ?? throw new InvalidOperationException("Campanha não encontrada");
        if (c.Status == CampaignStatus.Finalized) throw new InvalidOperationException("Chat somente leitura (campanha finalizada).");
        if (!await IsMemberAsync(campaignId, userId) && c.OwnerUserId != userId)
            throw new UnauthorizedAccessException("Apenas membros podem enviar mensagens.");

        var msg = new ChatMessage { CampaignId = campaignId, UserId = userId, DisplayName = displayName, Content = content, SentAsCharacter = sentAsCharacter };
        _db.ChatMessages.Add(msg);
        await _db.SaveChangesAsync();
        return msg;
    }

    public async Task<IReadOnlyList<ChatMessage>> ListChatMessagesAsync(Guid campaignId)
    {
        // SQLite does not support ordering by DateTimeOffset, so we order in-memory
        var list = await _db.ChatMessages
            .Where(m => m.CampaignId == campaignId)
            .ToListAsync();
        return list.OrderBy(m => m.CreatedAt).ToList();
    }

    public Task<bool> IsMemberAsync(Guid campaignId, string userId) =>
        _db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId);

    public async Task<bool> IsGmAsync(Guid campaignId, string userId)
    {
        var c = await _db.Campaigns.FindAsync(campaignId);
        return c != null && !string.IsNullOrEmpty(c.OwnerUserId) && c.OwnerUserId == userId;
    }

    public Task<int> CountMembersAsync(Guid campaignId) =>
        _db.CampaignMembers.CountAsync(m => m.CampaignId == campaignId && !m.IsBanned);

    private async Task Audit(string action, string actorUserId, Guid? campaignId, object data)
    {
        _db.AuditEntries.Add(new AuditEntry
        {
            ActorUserId = actorUserId,
            CampaignId = campaignId,
            ActionType = action,
            DataJson = JsonSerializer.Serialize(data)
        });
        await _db.SaveChangesAsync();
    }
}
