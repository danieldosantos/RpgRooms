using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RpgRooms.Core.Domain.Entities;
using RpgRooms.Infrastructure.Data;
using RpgRooms.Infrastructure.Services;
using Xunit;

public class CharacterEndpointsTests
{
    private static async Task<(CharacterService svc, Character character)> CriarFichaAsync(string dbName)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(dbName).Options;
        var db = new AppDbContext(opts);
        var svc = new CharacterService(db);
        var camp = new Campaign { Name = "Camp", OwnerUserId = "gm" };
        db.Campaigns.Add(camp);
        await db.SaveChangesAsync();
        var chr = new Character
        {
            CampaignId = camp.Id,
            UserId = "owner",
            Name = "Hero",
            Str = 10,
            Dex = 10,
            Con = 10,
            Int = 10,
            Wis = 10,
            Cha = 10
        };
        await svc.CreateCharacterAsync(chr);
        return (svc, chr);
    }

    [Fact]
    public async Task ProprietarioPodeEditar()
    {
        var (svc, chr) = await CriarFichaAsync("endp1");
        var atualizado = new Character { Name = "Novo", Str = 10, Dex = 10, Con = 10, Int = 10, Wis = 10, Cha = 10 };
        var sheet = await svc.UpdateCharacterAsync(chr.Id, atualizado, "owner");
        Assert.Equal("Novo", sheet.Character.Name);
    }

    [Fact]
    public async Task GmPodeEditar()
    {
        var (svc, chr) = await CriarFichaAsync("endp2");
        var atualizado = new Character { Name = "GMEdit", Str = 10, Dex = 10, Con = 10, Int = 10, Wis = 10, Cha = 10 };
        var sheet = await svc.UpdateCharacterAsync(chr.Id, atualizado, "gm");
        Assert.Equal("GMEdit", sheet.Character.Name);
    }

    [Fact]
    public async Task OutroUsuarioNaoPodeEditar()
    {
        var (svc, chr) = await CriarFichaAsync("endp3");
        var atualizado = new Character { Name = "Hack", Str = 10, Dex = 10, Con = 10, Int = 10, Wis = 10, Cha = 10 };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => svc.UpdateCharacterAsync(chr.Id, atualizado, "intruso"));
    }
}
