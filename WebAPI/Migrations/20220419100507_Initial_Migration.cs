using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI.Migrations
{
    public partial class Initial_Migration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    UploaderId = table.Column<long>(type: "bigint", nullable: false),
                    DateUploaded = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    DateDeleted = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    About = table.Column<string>(type: "varchar(4095)", maxLength: 4095, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhotoFileId = table.Column<long>(type: "bigint", nullable: true),
                    WallpaperFileId = table.Column<long>(type: "bigint", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    DateDeleted = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    LastConnected = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Availability = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Files_PhotoFileId",
                        column: x => x.PhotoFileId,
                        principalTable: "Files",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserProfiles_Files_WallpaperFileId",
                        column: x => x.WallpaperFileId,
                        principalTable: "Files",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChatRooms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    DateDeleted = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatRooms_UserProfiles_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DevicesUsed",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserProfileId = table.Column<long>(type: "bigint", nullable: false),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Timezone = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    DateUpdated = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    DateDeleted = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    PushNotificationToken = table.Column<string>(type: "varchar(1023)", maxLength: 1023, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateTokenProvided = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GroupProfiles",
                columns: table => new
                {
                    ChatRoomId = table.Column<long>(type: "bigint", nullable: false),
                    GroupName = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    JoinToken = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    About = table.Column<string>(type: "varchar(4095)", maxLength: 4095, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhotoFileId = table.Column<long>(type: "bigint", nullable: true),
                    WallpaperFileId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupProfiles", x => x.ChatRoomId);
                    table.ForeignKey(
                        name: "FK_GroupProfiles_ChatRooms_ChatRoomId",
                        column: x => x.ChatRoomId,
                        principalTable: "ChatRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupProfiles_Files_PhotoFileId",
                        column: x => x.PhotoFileId,
                        principalTable: "Files",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GroupProfiles_Files_WallpaperFileId",
                        column: x => x.WallpaperFileId,
                        principalTable: "Files",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserChatRooms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserProfileId = table.Column<long>(type: "bigint", nullable: false),
                    ChatRoomId = table.Column<long>(type: "bigint", nullable: false),
                    UserRole = table.Column<int>(type: "int", nullable: false),
                    AdderId = table.Column<long>(type: "bigint", nullable: true),
                    BlockerId = table.Column<long>(type: "bigint", nullable: true),
                    DateAdded = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    DateBlocked = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    DateDeleted = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    DateExited = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    DateMuted = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    DatePinned = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChatRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserChatRooms_ChatRooms_ChatRoomId",
                        column: x => x.ChatRoomId,
                        principalTable: "ChatRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserChatRooms_UserProfiles_AdderId",
                        column: x => x.AdderId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserChatRooms_UserProfiles_BlockerId",
                        column: x => x.BlockerId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserChatRooms_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MessageTags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChatRoomId = table.Column<long>(type: "bigint", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    CreatorId = table.Column<long>(type: "bigint", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    DateDeleted = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageTags_ChatRooms_ChatRoomId",
                        column: x => x.ChatRoomId,
                        principalTable: "ChatRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageTags_MessageTags_ParentId",
                        column: x => x.ParentId,
                        principalTable: "MessageTags",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MessageTags_UserChatRooms_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "UserChatRooms",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MessagesSent",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SenderId = table.Column<long>(type: "bigint", nullable: true),
                    MessageTagId = table.Column<long>(type: "bigint", nullable: false),
                    MessageType = table.Column<int>(type: "int", nullable: false),
                    LinkedId = table.Column<long>(type: "bigint", nullable: true),
                    AuthorId = table.Column<long>(type: "bigint", nullable: true),
                    FileId = table.Column<long>(type: "bigint", nullable: true),
                    Body = table.Column<string>(type: "longtext", maxLength: 16383, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateSent = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    DateDeleted = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    DateStarred = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessagesSent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessagesSent_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MessagesSent_MessagesSent_LinkedId",
                        column: x => x.LinkedId,
                        principalTable: "MessagesSent",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MessagesSent_MessageTags_MessageTagId",
                        column: x => x.MessageTagId,
                        principalTable: "MessageTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessagesSent_UserChatRooms_SenderId",
                        column: x => x.SenderId,
                        principalTable: "UserChatRooms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MessagesSent_UserProfiles_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MessagesReceived",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReceiverId = table.Column<long>(type: "bigint", nullable: false),
                    MessageSentId = table.Column<long>(type: "bigint", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    DateReceived = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    DateRead = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    DateDeleted = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    DateStarred = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    Reaction = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessagesReceived", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessagesReceived_MessagesSent_MessageSentId",
                        column: x => x.MessageSentId,
                        principalTable: "MessagesSent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessagesReceived_UserChatRooms_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "UserChatRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_CreatorId_Type",
                table: "ChatRooms",
                columns: new[] { "CreatorId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_DevicesUsed_UserProfileId_Platform",
                table: "DevicesUsed",
                columns: new[] { "UserProfileId", "Platform" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_Name",
                table: "Files",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupProfiles_PhotoFileId",
                table: "GroupProfiles",
                column: "PhotoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupProfiles_WallpaperFileId",
                table: "GroupProfiles",
                column: "WallpaperFileId");

            migrationBuilder.CreateIndex(
                name: "IX_MessagesReceived_MessageSentId",
                table: "MessagesReceived",
                column: "MessageSentId");

            migrationBuilder.CreateIndex(
                name: "IX_MessagesReceived_ReceiverId_MessageSentId",
                table: "MessagesReceived",
                columns: new[] { "ReceiverId", "MessageSentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessagesSent_AuthorId",
                table: "MessagesSent",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_MessagesSent_FileId",
                table: "MessagesSent",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_MessagesSent_LinkedId",
                table: "MessagesSent",
                column: "LinkedId");

            migrationBuilder.CreateIndex(
                name: "IX_MessagesSent_MessageTagId",
                table: "MessagesSent",
                column: "MessageTagId");

            migrationBuilder.CreateIndex(
                name: "IX_MessagesSent_SenderId",
                table: "MessagesSent",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageTags_ChatRoomId_Name",
                table: "MessageTags",
                columns: new[] { "ChatRoomId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageTags_CreatorId",
                table: "MessageTags",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageTags_ParentId",
                table: "MessageTags",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChatRooms_AdderId",
                table: "UserChatRooms",
                column: "AdderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChatRooms_BlockerId",
                table: "UserChatRooms",
                column: "BlockerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChatRooms_ChatRoomId",
                table: "UserChatRooms",
                column: "ChatRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChatRooms_UserProfileId_ChatRoomId",
                table: "UserChatRooms",
                columns: new[] { "UserProfileId", "ChatRoomId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Name",
                table: "UserProfiles",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_PhotoFileId",
                table: "UserProfiles",
                column: "PhotoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_WallpaperFileId",
                table: "UserProfiles",
                column: "WallpaperFileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DevicesUsed");

            migrationBuilder.DropTable(
                name: "GroupProfiles");

            migrationBuilder.DropTable(
                name: "MessagesReceived");

            migrationBuilder.DropTable(
                name: "MessagesSent");

            migrationBuilder.DropTable(
                name: "MessageTags");

            migrationBuilder.DropTable(
                name: "UserChatRooms");

            migrationBuilder.DropTable(
                name: "ChatRooms");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Files");
        }
    }
}
