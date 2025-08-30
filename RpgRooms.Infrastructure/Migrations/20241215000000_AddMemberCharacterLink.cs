using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgRooms.Infrastructure.Migrations;

public partial class AddMemberCharacterLink : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "CharacterId",
            table: "CampaignMembers",
            type: "TEXT",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_CampaignMembers_CharacterId",
            table: "CampaignMembers",
            column: "CharacterId");

        migrationBuilder.AddForeignKey(
            name: "FK_CampaignMembers_Characters_CharacterId",
            table: "CampaignMembers",
            column: "CharacterId",
            principalTable: "Characters",
            principalColumn: "Id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_CampaignMembers_Characters_CharacterId",
            table: "CampaignMembers");

        migrationBuilder.DropIndex(
            name: "IX_CampaignMembers_CharacterId",
            table: "CampaignMembers");

        migrationBuilder.DropColumn(
            name: "CharacterId",
            table: "CampaignMembers");
    }
}
