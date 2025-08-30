using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgRooms.Infrastructure.Migrations;

public partial class AddCharacterStats : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "ArmorClass",
            table: "Characters",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "CurrentHP",
            table: "Characters",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "MaxHP",
            table: "Characters",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "TemporaryHP",
            table: "Characters",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "Initiative",
            table: "Characters",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "Speed",
            table: "Characters",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "HitDice",
            table: "Characters",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "DeathSaves",
            table: "Characters",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<bool>(
            name: "Inspiration",
            table: "Characters",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateTable(
            name: "Languages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Languages", x => x.Id);
                table.ForeignKey(
                    name: "FK_Languages_Characters_CharacterId",
                    column: x => x.CharacterId,
                    principalTable: "Characters",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Features",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Features", x => x.Id);
                table.ForeignKey(
                    name: "FK_Features_Characters_CharacterId",
                    column: x => x.CharacterId,
                    principalTable: "Characters",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Languages_CharacterId",
            table: "Languages",
            column: "CharacterId");

        migrationBuilder.CreateIndex(
            name: "IX_Features_CharacterId",
            table: "Features",
            column: "CharacterId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Languages");

        migrationBuilder.DropTable(
            name: "Features");

        migrationBuilder.DropColumn(
            name: "ArmorClass",
            table: "Characters");

        migrationBuilder.DropColumn(
            name: "CurrentHP",
            table: "Characters");

        migrationBuilder.DropColumn(
            name: "MaxHP",
            table: "Characters");

        migrationBuilder.DropColumn(
            name: "TemporaryHP",
            table: "Characters");

        migrationBuilder.DropColumn(
            name: "Initiative",
            table: "Characters");

        migrationBuilder.DropColumn(
            name: "Speed",
            table: "Characters");

        migrationBuilder.DropColumn(
            name: "HitDice",
            table: "Characters");

        migrationBuilder.DropColumn(
            name: "DeathSaves",
            table: "Characters");

        migrationBuilder.DropColumn(
            name: "Inspiration",
            table: "Characters");
    }
}
