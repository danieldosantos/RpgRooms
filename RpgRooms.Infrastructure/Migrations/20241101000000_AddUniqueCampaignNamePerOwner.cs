using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgRooms.Infrastructure.Migrations;

public partial class AddUniqueCampaignNamePerOwner : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Campaigns_OwnerUserId_Name",
            table: "Campaigns",
            columns: new[] { "OwnerUserId", "Name" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Campaigns_OwnerUserId_Name",
            table: "Campaigns");
    }
}
