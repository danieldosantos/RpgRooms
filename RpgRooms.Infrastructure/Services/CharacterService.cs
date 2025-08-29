using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using RpgRooms.Core.Application.DTOs;
using RpgRooms.Core.Application.Interfaces;
using RpgRooms.Core.Domain.Entities;
using RpgRooms.Infrastructure.Data;

namespace RpgRooms.Infrastructure.Services;

public class CharacterService : ICharacterService
{
    private readonly AppDbContext _db;

    public CharacterService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CharacterSheetDto> CreateCharacterAsync(Character character)
    {
        foreach (var p in character.SavingThrowProficiencies)
            p.CharacterId = character.Id;
        foreach (var p in character.SkillProficiencies)
            p.CharacterId = character.Id;

        _db.Characters.Add(character);
        await _db.SaveChangesAsync();
        return BuildSheet(character);
    }

    public async Task<CharacterSheetDto> UpdateCharacterAsync(Guid id, Character character, string userId)
    {
        var existing = await _db.Characters
            .Include(c => c.SavingThrowProficiencies)
            .Include(c => c.SkillProficiencies)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new InvalidOperationException("Ficha não encontrada");

        var campaign = await _db.Campaigns.FindAsync(existing.CampaignId);
        if (existing.UserId != userId && campaign?.OwnerUserId != userId)
            throw new UnauthorizedAccessException("Apenas o dono ou o GM podem atualizar a ficha.");

        existing.Name = character.Name;
        existing.Race = character.Race;
        existing.Class = character.Class;
        existing.Level = character.Level;
        existing.Background = character.Background;
        existing.Alignment = character.Alignment;
        existing.XP = character.XP;
        existing.Str = character.Str;
        existing.Dex = character.Dex;
        existing.Con = character.Con;
        existing.Int = character.Int;
        existing.Wis = character.Wis;
        existing.Cha = character.Cha;

        _db.SavingThrowProficiencies.RemoveRange(existing.SavingThrowProficiencies);
        _db.SkillProficiencies.RemoveRange(existing.SkillProficiencies);
        await _db.SaveChangesAsync();

        existing.SavingThrowProficiencies = character.SavingThrowProficiencies
            .Select(p => new SavingThrowProficiency { CharacterId = existing.Id, Name = p.Name })
            .ToList();
        existing.SkillProficiencies = character.SkillProficiencies
            .Select(p => new SkillProficiency { CharacterId = existing.Id, Name = p.Name })
            .ToList();

        await _db.SaveChangesAsync();
        return BuildSheet(existing);
    }

    public async Task<CharacterSheetDto?> GetCharacterAsync(Guid id)
    {
        var c = await _db.Characters
            .Include(c => c.SavingThrowProficiencies)
            .Include(c => c.SkillProficiencies)
            .FirstOrDefaultAsync(c => c.Id == id);
        return c is null ? null : BuildSheet(c);
    }

    private CharacterSheetDto BuildSheet(Character c)
    {
        var modifiers = new Dictionary<string, int>
        {
            ["Str"] = Mod(c.Str),
            ["Dex"] = Mod(c.Dex),
            ["Con"] = Mod(c.Con),
            ["Int"] = Mod(c.Int),
            ["Wis"] = Mod(c.Wis),
            ["Cha"] = Mod(c.Cha)
        };

        var proficiency = 2 + (c.Level - 1) / 4;

        var saveProfs = c.SavingThrowProficiencies.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var savingThrows = new Dictionary<string, int>();
        foreach (var ab in modifiers.Keys)
        {
            var val = modifiers[ab];
            if (saveProfs.Contains(ab)) val += proficiency;
            savingThrows[ab] = val;
        }

        var skillAbilities = new Dictionary<string, string>
        {
            ["Acrobatics"] = "Dex",
            ["Animal Handling"] = "Wis",
            ["Arcana"] = "Int",
            ["Athletics"] = "Str",
            ["Deception"] = "Cha",
            ["History"] = "Int",
            ["Insight"] = "Wis",
            ["Intimidation"] = "Cha",
            ["Investigation"] = "Int",
            ["Medicine"] = "Wis",
            ["Nature"] = "Int",
            ["Perception"] = "Wis",
            ["Performance"] = "Cha",
            ["Persuasion"] = "Cha",
            ["Religion"] = "Int",
            ["Sleight of Hand"] = "Dex",
            ["Stealth"] = "Dex",
            ["Survival"] = "Wis"
        };

        var skillProfs = c.SkillProficiencies.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var skills = new Dictionary<string, int>();
        foreach (var kv in skillAbilities)
        {
            var val = modifiers[kv.Value];
            if (skillProfs.Contains(kv.Key)) val += proficiency;
            skills[kv.Key] = val;
        }

        var initiative = modifiers["Dex"];
        var spellDc = 8 + proficiency + modifiers[GetCastingAbility(c)];

        return new CharacterSheetDto(c, modifiers, savingThrows, skills, initiative, spellDc, proficiency);
    }

    private static int Mod(int score) => (int)Math.Floor((score - 10) / 2.0);

    private static string GetCastingAbility(Character c)
    {
        if (string.IsNullOrWhiteSpace(c.Class))
            return HighestMentalAbility(c);
        switch (c.Class.Trim().ToLowerInvariant())
        {
            case "wizard":
            case "artificer":
                return "Int";
            case "cleric":
            case "druid":
            case "ranger":
                return "Wis";
            case "bard":
            case "paladin":
            case "sorcerer":
            case "warlock":
                return "Cha";
            default:
                return HighestMentalAbility(c);
        }
    }

    private static string HighestMentalAbility(Character c)
    {
        var dict = new Dictionary<string, int>
        {
            ["Int"] = c.Int,
            ["Wis"] = c.Wis,
            ["Cha"] = c.Cha
        };
        return dict.OrderByDescending(kv => kv.Value).First().Key;
    }
}
