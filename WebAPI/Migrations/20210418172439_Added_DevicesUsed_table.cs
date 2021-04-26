using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace WebAPI.Migrations
{
    public partial class Added_DevicesUsed_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DevicesUsed",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<long>(type: "bigint", nullable: false),
                    DevicePlatform = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: DataType.DateTime, nullable: false),
                    DateDeleted = table.Column<DateTime>(type: DataType.DateTime, nullable: true),
                    PushNotificationToken = table.Column<string>(type: DataType.String(1023), maxLength: 1023, nullable: true),
                    DateTokenProvided = table.Column<DateTime>(type: DataType.DateTime, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicesUsed", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevicesUsed_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DevicesUsed_UserProfileId",
                table: "DevicesUsed",
                column: "UserProfileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DevicesUsed");
        }
    }
}
