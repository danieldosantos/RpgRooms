using System.ComponentModel.DataAnnotations;

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

    // Relationships
    public Guid CampaignId { get; set; }
    public string UserId { get; set; } = string.Empty;

    // Aggregates
    public ICollection<SavingThrowProficiency> SavingThrowProficiencies { get; set; } = new List<SavingThrowProficiency>();
    public ICollection<SkillProficiency> SkillProficiencies { get; set; } = new List<SkillProficiency>();
    public ICollection<InventoryItem> Inventory { get; set; } = new List<InventoryItem>();
    public ICollection<Spell> Spells { get; set; } = new List<Spell>();
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
