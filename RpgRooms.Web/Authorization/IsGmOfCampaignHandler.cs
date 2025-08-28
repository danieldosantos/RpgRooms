using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using RpgRooms.Infrastructure.Data;

namespace RpgRooms.Web.Authorization;

public class IsGmOfCampaignHandler : AuthorizationHandler<IsGmOfCampaignRequirement>
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;
    public IsGmOfCampaignHandler(AppDbContext db, IHttpContextAccessor? http = null)
    {
        _db = db; _http = http ?? new HttpContextAccessor();
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IsGmOfCampaignRequirement requirement)
    {
        var userId = context.User?.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userId)) return;

        var routeValues = _http.HttpContext?.GetRouteData()?.Values;
        if (routeValues is null) return;
        if (!routeValues.TryGetValue("id", out var idObj)) return;
        if (!Guid.TryParse(idObj?.ToString(), out var campaignId)) return;

        var campaign = await _db.Campaigns.FindAsync(campaignId);
        if (campaign != null && campaign.OwnerUserId == userId)
            context.Succeed(requirement);
    }
}
