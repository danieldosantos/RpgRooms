namespace RpgRooms.Core.Application.DTOs;

public record ChatMessageDto(Guid Id, string DisplayName, string Content, bool SentAsCharacter, DateTimeOffset CreatedAt);
