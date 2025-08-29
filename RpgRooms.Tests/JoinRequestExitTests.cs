using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RpgRooms.Infrastructure.Data;
using RpgRooms.Infrastructure.Services;
using Xunit;

public class JoinRequestExitTests
{
    [Fact]
    public async Task RejectsJoinRequestIfLeftWithin12Hours()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        using var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        var svc = new CampaignService(db);
        var camp = await svc.CreateCampaignAsync("gm", "A", null);
        await svc.ToggleRecruitmentAsync(camp.Id, "gm");
        db.CampaignMembers.Add(new() { CampaignId = camp.Id, UserId = "p1" });
        await db.SaveChangesAsync();
        await svc.LeaveCampaignAsync(camp.Id, "p1");
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CreateJoinRequestAsync(camp.Id, "p1", null));
    }

    [Fact]
    public async Task AllowsJoinRequestIfLeftMoreThan12HoursAgo()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        using var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        var svc = new CampaignService(db);
        var camp = await svc.CreateCampaignAsync("gm", "A", null);
        await svc.ToggleRecruitmentAsync(camp.Id, "gm");
        db.CampaignMembers.Add(new() { CampaignId = camp.Id, UserId = "p1" });
        await db.SaveChangesAsync();
        await svc.LeaveCampaignAsync(camp.Id, "p1");
        var exit = await db.CampaignExits.FirstAsync();
        exit.ExitedAt = DateTimeOffset.UtcNow.AddHours(-13);
        await db.SaveChangesAsync();
        var req = await svc.CreateJoinRequestAsync(camp.Id, "p1", null);
        Assert.NotNull(req);
    }
}
