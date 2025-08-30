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

        existing.SavingThrowProficiencies.Clear();
        existing.SkillProficiencies.Clear();

        existing.SavingThrowProficiencies.AddRange(
            character.SavingThrowProficiencies
                .Select(p => new SavingThrowProficiency { CharacterId = existing.Id, Name = p.Name })
        );

        existing.SkillProficiencies.AddRange(
            character.SkillProficiencies
                .Select(p => new SkillProficiency { CharacterId = existing.Id, Name = p.Name })
        );

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

    public async Task<IEnumerable<CharacterSheetDto>> GetCharactersAsync(Guid campaignId, string userId)
    {
        var chars = await _db.Characters
            .Include(c => c.SavingThrowProficiencies)
            .Include(c => c.SkillProficiencies)
            .Where(c => c.CampaignId == campaignId && c.UserId == userId)
            .ToListAsync();
        return chars.Select(BuildSheet);
    }

    public async Task DeleteCharacterAsync(Guid id, string userId)
    {
        var existing = await _db.Characters
            .Include(c => c.SavingThrowProficiencies)
            .Include(c => c.SkillProficiencies)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new InvalidOperationException("Ficha não encontrada");

        var campaign = await _db.Campaigns.FindAsync(existing.CampaignId);
        if (existing.UserId != userId && campaign?.OwnerUserId != userId)
            throw new UnauthorizedAccessException("Apenas o dono ou o GM podem excluir a ficha.");

        _db.SavingThrowProficiencies.RemoveRange(existing.SavingThrowProficiencies);
        _db.SkillProficiencies.RemoveRange(existing.SkillProficiencies);
        _db.Characters.Remove(existing);
        await _db.SaveChangesAsync();
    }

    private CharacterSheetDto BuildSheet(Character c)
    {
        var modifiers = new Dictionary<string, int>
        {
            ["Str"] = c.GetAbilityModifier("Str"),
            ["Dex"] = c.GetAbilityModifier("Dex"),
            ["Con"] = c.GetAbilityModifier("Con"),
            ["Int"] = c.GetAbilityModifier("Int"),
            ["Wis"] = c.GetAbilityModifier("Wis"),
            ["Cha"] = c.GetAbilityModifier("Cha")
        };

        var proficiency = c.GetProficiencyBonus();

        var savingThrows = new Dictionary<string, int>();
        foreach (var ab in modifiers.Keys)
            savingThrows[ab] = c.GetSavingThrow(ab);

        var skillNames = new[]
        {
            "Acrobatics", "Animal Handling", "Arcana", "Athletics", "Deception",
            "History", "Insight", "Intimidation", "Investigation", "Medicine",
            "Nature", "Perception", "Performance", "Persuasion", "Religion",
            "Sleight of Hand", "Stealth", "Survival"
        };
        var skills = new Dictionary<string, int>();
        foreach (var name in skillNames)
            skills[name] = c.GetSkillValue(name);

        var initiative = c.GetAbilityModifier("Dex");
        var spellDc = c.GetSpellSaveDC();

        return new CharacterSheetDto(c, modifiers, savingThrows, skills, initiative, spellDc, proficiency);
    }
}
