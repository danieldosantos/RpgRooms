using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RpgRooms.Infrastructure.Services;
using RpgRooms.Infrastructure.Data;
using Xunit;

public class CampaignRulesTests
{
    [Fact]
    public async Task CapNaoUltrapassa50()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("t").Options;
        var db = new AppDbContext(opts);
        var svc = new CampaignService(db);
        var camp = await svc.CreateCampaignAsync("gm","A","B");
        await svc.ToggleRecruitmentAsync(camp.Id, "gm");
        for(int i=0;i<50;i++)
            await db.CampaignMembers.AddAsync(new() { CampaignId = camp.Id, UserId = $"u{i}" });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CreateJoinRequestAsync(camp.Id, "u51", null));
    }
}
