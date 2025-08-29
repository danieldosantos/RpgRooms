using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgRooms.Infrastructure.Migrations;

public partial class AddCampaignExit : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CampaignExits",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CampaignId = table.Column<Guid>(type: "TEXT", nullable: false),
                UserId = table.Column<string>(type: "TEXT", nullable: false),
                ExitedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                IsKick = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CampaignExits", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CampaignExits_CampaignId_UserId",
            table: "CampaignExits",
            columns: new[] { "CampaignId", "UserId" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CampaignExits");
    }
}
