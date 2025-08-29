using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RpgRooms.Core.Domain.Entities;
using RpgRooms.Infrastructure.Data;
using RpgRooms.Infrastructure.Services;
using Xunit;

public class CharacterServiceTests
{
    [Fact]
    public async Task CalculaModificadoresSavesPericiasESpellDc()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("charsvc1").Options;
        var db = new AppDbContext(opts);
        var svc = new CharacterService(db);
        var character = new Character
        {
            UserId = "u1",
            CampaignId = Guid.NewGuid(),
            Str = 16,
            Dex = 14,
            Con = 12,
            Int = 10,
            Wis = 8,
            Cha = 18,
            Class = "sorcerer",
            SavingThrowProficiencies = new List<SavingThrowProficiency>
            {
                new() { Name = "Dex" },
                new() { Name = "Con" }
            },
            SkillProficiencies = new List<SkillProficiency>
            {
                new() { Name = "Acrobatics" },
                new() { Name = "Perception" },
                new() { Name = "Persuasion" }
            }
        };

        var sheet = await svc.CreateCharacterAsync(character);

        Assert.Equal(3, sheet.Modifiers["Str"]);
        Assert.Equal(2, sheet.Modifiers["Dex"]);
        Assert.Equal(1, sheet.Modifiers["Con"]);
        Assert.Equal(0, sheet.Modifiers["Int"]);
        Assert.Equal(-1, sheet.Modifiers["Wis"]);
        Assert.Equal(4, sheet.Modifiers["Cha"]);

        Assert.Equal(4, sheet.SavingThrows["Dex"]);
        Assert.Equal(3, sheet.SavingThrows["Con"]);
        Assert.Equal(-1, sheet.SavingThrows["Wis"]);

        Assert.Equal(4, sheet.Skills["Acrobatics"]);
        Assert.Equal(1, sheet.Skills["Perception"]);
        Assert.Equal(6, sheet.Skills["Persuasion"]);
        Assert.Equal(3, sheet.Skills["Athletics"]);

        Assert.Equal(14, sheet.SpellDc);
    }
}
