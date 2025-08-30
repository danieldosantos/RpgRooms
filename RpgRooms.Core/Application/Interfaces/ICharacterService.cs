using System;
using System.Collections.Generic;

using RpgRooms.Core.Application.DTOs;
using RpgRooms.Core.Domain.Entities;

namespace RpgRooms.Core.Application.Interfaces;

public interface ICharacterService
{
    Task<CharacterSheetDto> CreateCharacterAsync(Character character);
    Task<CharacterSheetDto> UpdateCharacterAsync(Guid id, Character character, string userId);
    Task<CharacterSheetDto?> GetCharacterAsync(Guid id);
    Task<IEnumerable<CharacterSheetDto>> GetCharactersAsync(Guid campaignId, string userId);
    Task DeleteCharacterAsync(Guid id, string userId);
}
