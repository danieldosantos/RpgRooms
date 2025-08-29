using Microsoft.AspNetCore.Authorization;
using RpgRooms.Core.Application.Interfaces;
using RpgRooms.Core.Domain.Entities;
using RpgRooms.Core.Security;
using System.Security.Claims;

namespace RpgRooms.Web.Endpoints;

public static class CharacterEndpoints
{
    public static IEndpointRouteBuilder MapCharacterEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/campaigns/{id:guid}/characters").RequireAuthorization();

        g.MapGet("{charId:guid}", async (Guid id, Guid charId, ICharacterService charSvc, IAuthorizationService auth, HttpContext http) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var sheet = await charSvc.GetCharacterAsync(charId);
            if (sheet is null || sheet.Character.CampaignId != id)
                return Results.NotFound();
            var isOwner = sheet.Character.UserId == userId;
            var isGm = (await auth.AuthorizeAsync(http.User, null, Policies.IsGmOfCampaign)).Succeeded;
            if (!isOwner && !isGm)
                return Results.Forbid();
            return Results.Ok(sheet);
        });

        g.MapPost("", async (Guid id, Character character, ICharacterService charSvc, ICampaignService campSvc, IAuthorizationService auth, HttpContext http) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isGm = (await auth.AuthorizeAsync(http.User, null, Policies.IsGmOfCampaign)).Succeeded;
            if (!isGm && !await campSvc.IsMemberAsync(id, userId))
                return Results.Forbid();
            character.UserId = userId;
            character.CampaignId = id;
            var sheet = await charSvc.CreateCharacterAsync(character);
            return Results.Ok(sheet);
        });

        g.MapPut("{charId:guid}", async (Guid id, Guid charId, Character character, ICharacterService charSvc, HttpContext http) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var sheet = await charSvc.UpdateCharacterAsync(charId, character, userId);
            if (sheet.Character.CampaignId != id)
                return Results.BadRequest();
            return Results.Ok(sheet);
        });

        g.MapDelete("{charId:guid}", async (Guid id, Guid charId, ICharacterService charSvc, IAuthorizationService auth, HttpContext http) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var sheet = await charSvc.GetCharacterAsync(charId);
            if (sheet is null || sheet.Character.CampaignId != id)
                return Results.NotFound();
            var isOwner = sheet.Character.UserId == userId;
            var isGm = (await auth.AuthorizeAsync(http.User, null, Policies.IsGmOfCampaign)).Succeeded;
            if (!isOwner && !isGm)
                return Results.Forbid();
            await charSvc.DeleteCharacterAsync(charId, userId);
            return Results.Ok();
        });

        return app;
    }
}
