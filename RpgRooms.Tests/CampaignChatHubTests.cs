using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RpgRooms.Infrastructure.Data;
using RpgRooms.Infrastructure.Services;
using RpgRooms.Web.Hubs;
using Xunit;

public class CampaignChatHubTests
{
    [Fact]
    public async Task JoinCampaignGroup_EmitsNoticeOnce()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("hubtest").Options;
        var db = new AppDbContext(opts);
        var svc = new CampaignService(db);
        var camp = await svc.CreateCampaignAsync("gm", "C", null);
        db.CampaignMembers.Add(new() { CampaignId = camp.Id, UserId = "u1" });
        await db.SaveChangesAsync();

        var hub = new CampaignChatHub(svc, db)
        {
            Clients = new TestHubCallerClients(),
            Groups = new TestGroupManager(),
            Context = new TestHubCallerContext("c1", "u1")
        };

        await hub.JoinCampaignGroup(camp.Id);
        await hub.JoinCampaignGroup(camp.Id);

        var proxy = (TestClientProxy)((TestHubCallerClients)hub.Clients).GroupProxy;
        var notices = proxy.Sent.Count(s => s.method == "SystemNotice");
        Assert.Equal(1, notices);
    }
}

class TestGroupManager : IGroupManager
{
    public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

class TestClientProxy : IClientProxy
{
    public List<(string method, object?[] args)> Sent { get; } = new();
    public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
    {
        Sent.Add((method, args));
        return Task.CompletedTask;
    }
}

class TestHubCallerClients : IHubCallerClients
{
    public IClientProxy GroupProxy { get; } = new TestClientProxy();
    public IClientProxy All => GroupProxy;
    public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => GroupProxy;
    public IClientProxy Client(string connectionId) => GroupProxy;
    public IClientProxy Clients(IReadOnlyList<string> connectionIds) => GroupProxy;
    public IClientProxy Group(string groupName) => GroupProxy;
    public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => GroupProxy;
    public IClientProxy Groups(IReadOnlyList<string> groupNames) => GroupProxy;
    public IClientProxy User(string userId) => GroupProxy;
    public IClientProxy Users(IReadOnlyList<string> userIds) => GroupProxy;
    public IClientProxy Caller => GroupProxy;
    public IClientProxy Others => GroupProxy;
    public IClientProxy OthersInGroup(string groupName) => GroupProxy;
}

class TestHubCallerContext : HubCallerContext
{
    public override string ConnectionId { get; }
    public override string? UserIdentifier => User.FindFirstValue(ClaimTypes.NameIdentifier);
    public override ClaimsPrincipal User { get; }
    private readonly Dictionary<object, object?> _items = new();
    public override IDictionary<object, object?> Items => _items;
    public override IFeatureCollection Features { get; } = new FeatureCollection();
    public override CancellationToken ConnectionAborted { get; } = CancellationToken.None;
    public override void Abort() { }

    public TestHubCallerContext(string connectionId, string userId)
    {
        ConnectionId = connectionId;
        User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));
    }
}
