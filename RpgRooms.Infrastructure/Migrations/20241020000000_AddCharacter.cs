using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgRooms.Infrastructure.Migrations;

public partial class AddCharacter : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Characters",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CampaignId = table.Column<Guid>(type: "TEXT", nullable: false),
                UserId = table.Column<string>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                Race = table.Column<string>(type: "TEXT", nullable: true),
                Class = table.Column<string>(type: "TEXT", nullable: true),
                Level = table.Column<int>(type: "INTEGER", nullable: false),
                Background = table.Column<string>(type: "TEXT", nullable: true),
                Alignment = table.Column<string>(type: "TEXT", nullable: true),
                XP = table.Column<int>(type: "INTEGER", nullable: false),
                Str = table.Column<int>(type: "INTEGER", nullable: false),
                Dex = table.Column<int>(type: "INTEGER", nullable: false),
                Con = table.Column<int>(type: "INTEGER", nullable: false),
                Int = table.Column<int>(type: "INTEGER", nullable: false),
                Wis = table.Column<int>(type: "INTEGER", nullable: false),
                Cha = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Characters", x => x.Id);
                table.ForeignKey(
                    name: "FK_Characters_Campaigns_CampaignId",
                    column: x => x.CampaignId,
                    principalTable: "Campaigns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Characters_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "InventoryItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InventoryItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_InventoryItems_Characters_CharacterId",
                    column: x => x.CharacterId,
                    principalTable: "Characters",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SavingThrowProficiencies",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SavingThrowProficiencies", x => x.Id);
                table.ForeignKey(
                    name: "FK_SavingThrowProficiencies_Characters_CharacterId",
                    column: x => x.CharacterId,
                    principalTable: "Characters",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SkillProficiencies",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SkillProficiencies", x => x.Id);
                table.ForeignKey(
                    name: "FK_SkillProficiencies_Characters_CharacterId",
                    column: x => x.CharacterId,
                    principalTable: "Characters",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Spells",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Spells", x => x.Id);
                table.ForeignKey(
                    name: "FK_Spells_Characters_CharacterId",
                    column: x => x.CharacterId,
                    principalTable: "Characters",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Characters_CampaignId",
            table: "Characters",
            column: "CampaignId");

        migrationBuilder.CreateIndex(
            name: "IX_Characters_UserId",
            table: "Characters",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_InventoryItems_CharacterId",
            table: "InventoryItems",
            column: "CharacterId");

        migrationBuilder.CreateIndex(
            name: "IX_SavingThrowProficiencies_CharacterId",
            table: "SavingThrowProficiencies",
            column: "CharacterId");

        migrationBuilder.CreateIndex(
            name: "IX_SkillProficiencies_CharacterId",
            table: "SkillProficiencies",
            column: "CharacterId");

        migrationBuilder.CreateIndex(
            name: "IX_Spells_CharacterId",
            table: "Spells",
            column: "CharacterId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "InventoryItems");

        migrationBuilder.DropTable(
            name: "SavingThrowProficiencies");

        migrationBuilder.DropTable(
            name: "SkillProficiencies");

        migrationBuilder.DropTable(
            name: "Spells");

        migrationBuilder.DropTable(
            name: "Characters");
    }
}
