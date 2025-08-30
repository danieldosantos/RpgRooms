using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace RpgRooms.Core.Domain.Entities;

public class Character
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Identification
    [Required, StringLength(80, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;
    [StringLength(40)]
    public string? Race { get; set; }
    [StringLength(40)]
    public string? Class { get; set; }
    public int Level { get; set; } = 1;
    [StringLength(100)]
    public string? Background { get; set; }
    [StringLength(40)]
    public string? Alignment { get; set; }
    public int XP { get; set; }

    // Attributes
    public int Str { get; set; }
    public int Dex { get; set; }
    public int Con { get; set; }
    public int Int { get; set; }
    public int Wis { get; set; }
    public int Cha { get; set; }

    // Combat stats
    public int ArmorClass { get; set; }
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }
    public int TemporaryHP { get; set; }
    // Extra initiative bonus besides Dex modifier
    public int Initiative { get; set; }
    public int Speed { get; set; }
    [StringLength(20)]
    public string? HitDice { get; set; }
    public int DeathSaves { get; set; }
    public bool Inspiration { get; set; }

    // Relationships
    public Guid CampaignId { get; set; }
    public string UserId { get; set; } = string.Empty;

    // Aggregates
    public ICollection<SavingThrowProficiency> SavingThrowProficiencies { get; set; } = new List<SavingThrowProficiency>();
    public ICollection<SkillProficiency> SkillProficiencies { get; set; } = new List<SkillProficiency>();
    public ICollection<InventoryItem> Inventory { get; set; } = new List<InventoryItem>();
    public ICollection<Spell> Spells { get; set; } = new List<Spell>();
    public ICollection<Language> Languages { get; set; } = new List<Language>();
    public ICollection<Feature> Features { get; set; } = new List<Feature>();

    public int GetAbilityModifier(string ability)
    {
        var score = ability switch
        {
            "Str" => Str,
            "Dex" => Dex,
            "Con" => Con,
            "Int" => Int,
            "Wis" => Wis,
            "Cha" => Cha,
            _ => 10
        };
        return (int)Math.Floor((score - 10) / 2.0);
    }

    public int GetProficiencyBonus() => 2 + (Level - 1) / 4;

    public int GetSavingThrow(string ability)
    {
        var total = GetAbilityModifier(ability);
        if (SavingThrowProficiencies.Any(p => p.Name.Equals(ability, StringComparison.OrdinalIgnoreCase)))
            total += GetProficiencyBonus();
        return total;
    }

    private static readonly IDictionary<string, string> SkillAbilities = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Acrobatics"] = "Dex",
        ["Acrobacia"] = "Dex",
        ["Animal Handling"] = "Wis",
        ["Arcanismo"] = "Int",
        ["Arcana"] = "Int",
        ["Athletics"] = "Str",
        ["Atletismo"] = "Str",
        ["Deception"] = "Cha",
        ["História"] = "Int",
        ["History"] = "Int",
        ["Insight"] = "Wis",
        ["Intimidação"] = "Cha",
        ["Intimidation"] = "Cha",
        ["Investigation"] = "Int",
        ["Medicine"] = "Wis",
        ["Nature"] = "Int",
        ["Perception"] = "Wis",
        ["Percepção"] = "Wis",
        ["Performance"] = "Cha",
        ["Persuasion"] = "Cha",
        ["Religion"] = "Int",
        ["Furtividade"] = "Dex",
        ["Stealth"] = "Dex",
        ["Sobrevivência"] = "Wis",
        ["Survival"] = "Wis",
        ["Sleight of Hand"] = "Dex"
    };

    public int GetSkillValue(string skill)
    {
        if (!SkillAbilities.TryGetValue(skill, out var ability))
            throw new ArgumentException($"Unknown skill {skill}", nameof(skill));
        var total = GetAbilityModifier(ability);
        if (SkillProficiencies.Any(p => p.Name.Equals(skill, StringComparison.OrdinalIgnoreCase)))
            total += GetProficiencyBonus();
        return total;
    }

    public int GetPassivePerception() => 10 + GetSkillValue("Percepção");

    public int GetSpellSaveDC()
        => 8 + GetProficiencyBonus() + GetAbilityModifier(GetCastingAbility());

    private string GetCastingAbility()
    {
        if (string.IsNullOrWhiteSpace(Class))
            return HighestMentalAbility();
        switch (Class.Trim().ToLowerInvariant())
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
                return HighestMentalAbility();
        }
    }

    private string HighestMentalAbility()
    {
        var dict = new Dictionary<string, int>
        {
            ["Int"] = Int,
            ["Wis"] = Wis,
            ["Cha"] = Cha
        };
        return dict.OrderByDescending(kv => kv.Value).First().Key;
    }
}

public class SavingThrowProficiency
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CharacterId { get; set; }
    [Required, StringLength(40)]
    public string Name { get; set; } = string.Empty;
}

public class SkillProficiency
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CharacterId { get; set; }
    [Required, StringLength(40)]
    public string Name { get; set; } = string.Empty;
}

public class InventoryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CharacterId { get; set; }
    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;
}

public class Spell
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CharacterId { get; set; }
    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;
}

public class Language
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CharacterId { get; set; }
    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;
}

public class Feature
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CharacterId { get; set; }
    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;
}
