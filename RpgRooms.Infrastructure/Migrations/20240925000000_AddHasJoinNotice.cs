using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgRooms.Infrastructure.Migrations;

public partial class AddHasJoinNotice : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "HasJoinNotice",
            table: "CampaignMembers",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "HasJoinNotice",
            table: "CampaignMembers");
    }
}
