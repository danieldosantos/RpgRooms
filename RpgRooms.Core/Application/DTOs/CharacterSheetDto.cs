using RpgRooms.Core.Domain.Entities;
using System.Collections.Generic;

namespace RpgRooms.Core.Application.DTOs;

public record CharacterSheetDto(
    Character Character,
    IDictionary<string, int> Modifiers,
    IDictionary<string, int> SavingThrows,
    IDictionary<string, int> Skills,
    int Initiative,
    int SpellDc,
    int ProficiencyBonus,
    int ArmorClass,
    int CurrentHP,
    int MaxHP,
    int TemporaryHP,
    int Speed,
    string? HitDice,
    int DeathSaves,
    bool Inspiration,
    IEnumerable<string> Languages,
    IEnumerable<string> Features);
