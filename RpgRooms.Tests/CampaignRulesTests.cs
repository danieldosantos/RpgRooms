using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RpgRooms.Infrastructure.Services;
using RpgRooms.Infrastructure.Data;
using RpgRooms.Core.Domain.Enums;
using RpgRooms.Core.Domain.Entities;
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

    [Fact]
    public async Task NaoPermiteReentrarAntesDe12h()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("t7").Options;
        var db = new AppDbContext(opts);
        var svc = new CampaignService(db);
        var camp = await svc.CreateCampaignAsync("gm", "A", null);
        await svc.ToggleRecruitmentAsync(camp.Id, "gm");
        db.CampaignMembers.Add(new() { CampaignId = camp.Id, UserId = "p1" });
        await db.SaveChangesAsync();
        await svc.LeaveCampaignAsync(camp.Id, "p1");
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CreateJoinRequestAsync(camp.Id, "p1", null));
    }

    [Fact]
    public async Task PermiteReentrarApos12h()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("t8").Options;
        var db = new AppDbContext(opts);
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

    [Fact]
    public async Task AprovarJoinNaoCriaCharacter()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("t9").Options;
        var db = new AppDbContext(opts);
        var svc = new CampaignService(db);
        var camp = await svc.CreateCampaignAsync("gm", "A", null);
        await svc.ToggleRecruitmentAsync(camp.Id, "gm");
        var req = await svc.CreateJoinRequestAsync(camp.Id, "p1", null);
        await svc.ApproveJoinRequestAsync(camp.Id, req.Id, "gm");
        Assert.Equal(0, await db.Characters.CountAsync());
    }

    [Fact]
    public async Task HandleExitRemoveCharacters()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("t10").Options;
        var db = new AppDbContext(opts);
        var svc = new CampaignService(db);
        var camp = await svc.CreateCampaignAsync("gm", "A", null);
        await svc.ToggleRecruitmentAsync(camp.Id, "gm");
        var req = await svc.CreateJoinRequestAsync(camp.Id, "p1", null);
        await svc.ApproveJoinRequestAsync(camp.Id, req.Id, "gm");
        var charSvc = new CharacterService(db);
        await charSvc.CreateCharacterAsync(new Character { CampaignId = camp.Id, UserId = "p1", Name = "Hero" });
        await svc.LeaveCampaignAsync(camp.Id, "p1");
        await svc.HandleCharacterExitAsync(camp.Id, "p1", "gm", null);
        Assert.Equal(0, await db.Characters.CountAsync());
    }

    [Fact]
    public async Task HandleExitTransferCharacters()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("t11").Options;
        var db = new AppDbContext(opts);
        var svc = new CampaignService(db);
        var camp = await svc.CreateCampaignAsync("gm", "A", null);
        await svc.ToggleRecruitmentAsync(camp.Id, "gm");
        var req1 = await svc.CreateJoinRequestAsync(camp.Id, "p1", null);
        await svc.ApproveJoinRequestAsync(camp.Id, req1.Id, "gm");
        var req2 = await svc.CreateJoinRequestAsync(camp.Id, "p2", null);
        await svc.ApproveJoinRequestAsync(camp.Id, req2.Id, "gm");
        var charSvc = new CharacterService(db);
        var sheet = await charSvc.CreateCharacterAsync(new Character { CampaignId = camp.Id, UserId = "p1", Name = "Hero" });
        await svc.LeaveCampaignAsync(camp.Id, "p1");
        await svc.HandleCharacterExitAsync(camp.Id, "p1", "gm", "p2");
        var updated = await db.Characters.FirstAsync(c => c.Id == sheet.Character.Id);
        Assert.Equal("p2", updated.UserId);
    }

    [Fact]
    public async Task SetMemberCharacterAtualizaNome()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("t12").Options;
        var db = new AppDbContext(opts);
        var svc = new CampaignService(db);
        var camp = await svc.CreateCampaignAsync("gm", "A", null);
        db.CampaignMembers.Add(new() { CampaignId = camp.Id, UserId = "p1" });
        var ch = new Character
        {
            CampaignId = camp.Id,
            UserId = "p1",
            Name = "Hero",
            Level = 1,
            Str = 10,
            Dex = 10,
            Con = 10,
            Int = 10,
            Wis = 10,
            Cha = 10
        };
        db.Characters.Add(ch);
        await db.SaveChangesAsync();
        await svc.SetMemberCharacterAsync(camp.Id, "p1", ch.Id, "gm");
        var member = await db.CampaignMembers.FirstAsync(m => m.CampaignId == camp.Id && m.UserId == "p1");
        Assert.Equal("Hero", member.CharacterName);
        Assert.Equal(ch.Id, member.CharacterId);
    }
}
