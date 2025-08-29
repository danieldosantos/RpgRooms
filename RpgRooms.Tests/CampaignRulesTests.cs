using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RpgRooms.Infrastructure.Services;
using RpgRooms.Infrastructure.Data;
using RpgRooms.Core.Domain.Enums;
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

    [Fact]
    public async Task NaoCriaCampanhaDuplicada()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("t2").Options;
        var db = new AppDbContext(opts);
        var svc = new CampaignService(db);
        var camp1 = await svc.CreateCampaignAsync("gm", "A", "desc1");
        var camp2 = await svc.CreateCampaignAsync("gm", "A", "desc2");
        Assert.Equal(camp1.Id, camp2.Id);
        Assert.Equal(1, await db.Campaigns.CountAsync());
    }

    [Fact]
    public async Task StatusInicialEhInProgress()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("t3").Options;
        var db = new AppDbContext(opts);
        var svc = new CampaignService(db);
        var camp = await svc.CreateCampaignAsync("gm", "A", "B");
        Assert.Equal(CampaignStatus.InProgress, camp.Status);
    }

    [Fact]
    public async Task ListaPadraoNaoIncluiAtivasFechadas()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("t4").Options;
        var db = new AppDbContext(opts);
        var svc = new CampaignService(db);

        var ativaFechada = await svc.CreateCampaignAsync("gm1", "Ativa", null);

        var recrutando = await svc.CreateCampaignAsync("gm2", "Recrutando", null);
        await svc.ToggleRecruitmentAsync(recrutando.Id, "gm2");

        var finalizada = await svc.CreateCampaignAsync("gm3", "Finalizada", null);
        await svc.FinalizeCampaignAsync(finalizada.Id, "gm3");

        var lista = await svc.ListCampaignsAsync(null, false, null, null);

        Assert.DoesNotContain(lista, c => c.Id == ativaFechada.Id);
        Assert.Contains(lista, c => c.Id == recrutando.Id);
        Assert.Contains(lista, c => c.Id == finalizada.Id);
    }

    [Fact]
    public async Task FinalizarRemoveMembros()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("t5").Options;
        var db = new AppDbContext(opts);
        var svc = new CampaignService(db);
        var camp = await svc.CreateCampaignAsync("gm", "A", null);
        db.CampaignMembers.Add(new() { CampaignId = camp.Id, UserId = "p1" });
        db.CampaignMembers.Add(new() { CampaignId = camp.Id, UserId = "p2" });
        await db.SaveChangesAsync();

        await svc.FinalizeCampaignAsync(camp.Id, "gm");

        Assert.Equal(0, await db.CampaignMembers.CountAsync(m => m.CampaignId == camp.Id));
    }

    [Fact]
    public async Task IsGmRetornaFalseAposFinalizar()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("t6").Options;
        var db = new AppDbContext(opts);
        var svc = new CampaignService(db);
        var camp = await svc.CreateCampaignAsync("gm", "A", null);
        Assert.True(await svc.IsGmAsync(camp.Id, "gm"));
        await svc.FinalizeCampaignAsync(camp.Id, "gm");
        Assert.False(await svc.IsGmAsync(camp.Id, "gm"));
    }
}
