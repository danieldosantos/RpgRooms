using RpgRooms.Core.Application.Interfaces;
using RpgRooms.Core.Application.DTOs;

namespace RpgRooms.Web.Endpoints;

public static class CampaignEndpoints
{
    public static IEndpointRouteBuilder MapCampaignEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/campaigns").RequireAuthorization();

        g.MapPost("", async (ICampaignService svc, HttpContext http, CreateCampaignDto dto) =>
        {
            var userId = http.User.Identity!.Name!;
            var c = await svc.CreateCampaignAsync(userId, dto.Name, dto.Description);
            return Results.Ok(c);
        });

        g.MapPut("{id:guid}/recruitment/toggle", async (Guid id, ICampaignService svc, HttpContext http) =>
        {
            var userId = http.User.Identity!.Name!;
            var state = await svc.ToggleRecruitmentAsync(id, userId);
            return Results.Ok(new { isRecruiting = state });
        });

        g.MapPut("{id:guid}/finalize", async (Guid id, ICampaignService svc, HttpContext http) =>
        {
            var userId = http.User.Identity!.Name!;
            await svc.FinalizeCampaignAsync(id, userId);
            return Results.Ok();
        });

        g.MapPost("{id:guid}/join-requests", async (Guid id, ICampaignService svc, HttpContext http, CreateJoinRequestDto dto) =>
        {
            var userId = http.User.Identity!.Name!;
            var r = await svc.CreateJoinRequestAsync(id, userId, dto.Message);
            return Results.Ok(r);
        });

        g.MapPut("{id:guid}/join-requests/{reqId:guid}/approve", async (Guid id, Guid reqId, ICampaignService svc, HttpContext http) =>
        {
            var userId = http.User.Identity!.Name!;
            await svc.ApproveJoinRequestAsync(id, reqId, userId);
            return Results.Ok();
        });

        g.MapPut("{id:guid}/join-requests/{reqId:guid}/reject", async (Guid id, Guid reqId, ICampaignService svc, HttpContext http) =>
        {
            var userId = http.User.Identity!.Name!;
            await svc.RejectJoinRequestAsync(id, reqId, userId);
            return Results.Ok();
        });

        g.MapGet("{id:guid}/join-requests", async (Guid id, ICampaignService svc, HttpContext http) =>
        {
            var userId = http.User.Identity!.Name!;
            var list = await svc.ListJoinRequestsAsync(id, userId);
            return Results.Ok(list);
        });

        g.MapGet("{id:guid}/members", async (Guid id, ICampaignService svc, HttpContext http) =>
        {
            var userId = http.User.Identity!.Name!;
            var list = await svc.ListMembersAsync(id, userId);
            return Results.Ok(list);
        });

        g.MapGet("{id:guid}/is-member", async (Guid id, ICampaignService svc, HttpContext http) =>
        {
            var userId = http.User.Identity!.Name!;
            var isMember = await svc.IsMemberAsync(id, userId);
            return Results.Ok(isMember);
        });

        g.MapGet("{id:guid}/messages", async (Guid id, ICampaignService svc, HttpContext http) =>
        {
            var userId = http.User.Identity!.Name!;
            if (!await svc.IsMemberAsync(id, userId) && !await svc.IsGmAsync(id, userId))
                return Results.Forbid();
            var list = await svc.ListChatMessagesAsync(id);
            var dtos = list.Select(m => new ChatMessageDto(m.Id, m.DisplayName, m.Content, m.SentAsCharacter));
            return Results.Ok(dtos);
        });

        g.MapDelete("{id:guid}/members/{targetUserId}", async (Guid id, string targetUserId, ICampaignService svc, HttpContext http, string? reason) =>
        {
            var userId = http.User.Identity!.Name!;
            await svc.RemoveMemberAsync(id, targetUserId, userId, reason);
            return Results.Ok();
        });

        g.MapDelete("{id:guid}/leave", async (Guid id, ICampaignService svc, HttpContext http) =>
        {
            var userId = http.User.Identity!.Name!;
            await svc.LeaveCampaignAsync(id, userId);
            return Results.Ok();
        });

        g.MapGet("mine", async (ICampaignService svc, HttpContext http) =>
        {
            var userId = http.User.Identity!.Name!;
            var list = await svc.ListUserCampaignsAsync(userId);
            return Results.Ok(list);
        });

        g.MapGet("", async (ICampaignService svc, string? search, bool recruitingOnly, string? ownerUserId, string? status) =>
        {
            var list = await svc.ListCampaignsAsync(search, recruitingOnly, ownerUserId, status);
            return Results.Ok(list);
        }).AllowAnonymous();

        g.MapGet("{id:guid}", async (Guid id, ICampaignService svc) =>
        {
            var c = await svc.GetCampaignAsync(id);
            return c is null ? Results.NotFound() : Results.Ok(c);
        }).AllowAnonymous();

        return app;
    }
}

public record CreateCampaignDto(string Name, string? Description);
public record CreateJoinRequestDto(string? Message);
