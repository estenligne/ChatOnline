using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace WebAPI.Migrations
{
    public partial class Initial_Migration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: DataType.String(256), maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: DataType.String(256), maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: DataType.String(256), maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: DataType.String(256), maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: DataType.String(256), maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: DataType.String(256), maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: DataType.String(256), maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: DataType.Bool, nullable: false),
                    PasswordHash = table.Column<string>(type: DataType.String(256), maxLength: 256, nullable: true),
                    SecurityStamp = table.Column<string>(type: DataType.String(256), maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: DataType.String(256), maxLength: 256, nullable: true),
                    PhoneNumber = table.Column<string>(type: DataType.String(256), maxLength: 256, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: DataType.Bool, nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: DataType.Bool, nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: DataType.Timestamp, nullable: true),
                    LockoutEnabled = table.Column<bool>(type: DataType.Bool, nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    ClaimType = table.Column<string>(type: DataType.String(-1), nullable: true),
                    ClaimValue = table.Column<string>(type: DataType.String(-1), nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ClaimType = table.Column<string>(type: DataType.String(-1), nullable: true),
                    ClaimValue = table.Column<string>(type: DataType.String(-1), nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: DataType.String(450), nullable: false),
                    ProviderKey = table.Column<string>(type: DataType.String(450), nullable: false),
                    ProviderDisplayName = table.Column<string>(type: DataType.String(-1), nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    LoginProvider = table.Column<string>(type: DataType.String(450), nullable: false),
                    Name = table.Column<string>(type: DataType.String(450), nullable: false),
                    Value = table.Column<string>(type: DataType.String(-1), nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: DataType.String(255), maxLength: 255, nullable: true),
                    Size = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    UploaderId = table.Column<long>(type: "bigint", nullable: false),
                    DateUploaded = table.Column<DateTime>(type: DataType.DateTime, nullable: false),
                    DateDeleted = table.Column<DateTime>(type: DataType.DateTime, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_AspNetUsers_UploaderId",
                        column: x => x.UploaderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Username = table.Column<string>(type: DataType.String(63), maxLength: 63, nullable: true),
                    Availability = table.Column<string>(type: DataType.String(63), maxLength: 63, nullable: true),
                    About = table.Column<string>(type: DataType.String(-1), maxLength: 4095, nullable: true),
                    PhotoFileId = table.Column<long>(type: "bigint", nullable: true),
                    WallpaperFileId = table.Column<long>(type: "bigint", nullable: true),
                    DateCreated = table.Column<DateTime>(type: DataType.DateTime, nullable: false),
                    DateDeleted = table.Column<DateTime>(type: DataType.DateTime, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Files_PhotoFileId",
                        column: x => x.PhotoFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Files_WallpaperFileId",
                        column: x => x.WallpaperFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupProfiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    Groupname = table.Column<string>(type: DataType.String(63), maxLength: 63, nullable: true),
                    About = table.Column<string>(type: DataType.String(-1), maxLength: 4095, nullable: true),
                    PhotoFileId = table.Column<long>(type: "bigint", nullable: true),
                    WallpaperFileId = table.Column<long>(type: "bigint", nullable: true),
                    DateCreated = table.Column<DateTime>(type: DataType.DateTime, nullable: false),
                    DateDeleted = table.Column<DateTime>(type: DataType.DateTime, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupProfiles_Files_PhotoFileId",
                        column: x => x.PhotoFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupProfiles_Files_WallpaperFileId",
                        column: x => x.WallpaperFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupProfiles_UserProfiles_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatRooms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    GroupProfileId = table.Column<long>(type: "bigint", nullable: true),
                    DateCreated = table.Column<DateTime>(type: DataType.DateTime, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatRooms_GroupProfiles_GroupProfileId",
                        column: x => x.GroupProfileId,
                        principalTable: "GroupProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessageTags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: DataType.String(63), maxLength: 63, nullable: true),
                    ChatRoomId = table.Column<long>(type: "bigint", nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    DateCreated = table.Column<DateTime>(type: DataType.DateTime, nullable: false),
                    IsPrivate = table.Column<bool>(type: DataType.Bool, nullable: false)
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
                        name: "FK_MessageTags_UserProfiles_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserChatRooms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<long>(type: "bigint", nullable: false),
                    ChatRoomId = table.Column<long>(type: "bigint", nullable: false),
                    UserRole = table.Column<int>(type: "int", nullable: false),
                    AdderId = table.Column<long>(type: "bigint", nullable: true),
                    BlockerId = table.Column<long>(type: "bigint", nullable: true),
                    DateAdded = table.Column<DateTime>(type: DataType.DateTime, nullable: false),
                    DateBlocked = table.Column<DateTime>(type: DataType.DateTime, nullable: true),
                    DateDeleted = table.Column<DateTime>(type: DataType.DateTime, nullable: true),
                    DateExited = table.Column<DateTime>(type: DataType.DateTime, nullable: true),
                    DateMuted = table.Column<DateTime>(type: DataType.DateTime, nullable: true),
                    MuteDuration = table.Column<TimeSpan>(type: "time", nullable: true)
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserChatRooms_UserProfiles_BlockerId",
                        column: x => x.BlockerId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserChatRooms_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessagesSent",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<long>(type: "bigint", nullable: false),
                    MessageTagId = table.Column<long>(type: "bigint", nullable: false),
                    MessageType = table.Column<int>(type: "int", nullable: false),
                    LinkedId = table.Column<long>(type: "bigint", nullable: true),
                    AuthorId = table.Column<long>(type: "bigint", nullable: true),
                    Body = table.Column<string>(type: DataType.String(-1), maxLength: 16383, nullable: true),
                    DateSent = table.Column<DateTime>(type: DataType.DateTime, nullable: false),
                    DateReceicedByServer = table.Column<DateTime>(type: DataType.DateTime, nullable: false),
                    DateDeleted = table.Column<DateTime>(type: DataType.DateTime, nullable: true),
                    DateStarred = table.Column<DateTime>(type: DataType.DateTime, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessagesSent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessagesSent_MessagesSent_LinkedId",
                        column: x => x.LinkedId,
                        principalTable: "MessagesSent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MessagesSent_UserProfiles_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessagesReceived",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiverId = table.Column<long>(type: "bigint", nullable: false),
                    MessageSentId = table.Column<long>(type: "bigint", nullable: false),
                    DateReceived = table.Column<DateTime>(type: DataType.DateTime, nullable: false),
                    DateRead = table.Column<DateTime>(type: DataType.DateTime, nullable: true),
                    DateDeleted = table.Column<DateTime>(type: DataType.DateTime, nullable: true),
                    DateStarred = table.Column<DateTime>(type: DataType.DateTime, nullable: true),
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_GroupProfileId",
                table: "ChatRooms",
                column: "GroupProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_UploaderId",
                table: "Files",
                column: "UploaderId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupProfiles_CreatorId",
                table: "GroupProfiles",
                column: "CreatorId");

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
                name: "IX_MessagesReceived_ReceiverId",
                table: "MessagesReceived",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_MessagesSent_AuthorId",
                table: "MessagesSent",
                column: "AuthorId");

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
                name: "IX_MessageTags_ChatRoomId",
                table: "MessageTags",
                column: "ChatRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageTags_CreatorId",
                table: "MessageTags",
                column: "CreatorId");

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
                name: "IX_UserChatRooms_UserProfileId",
                table: "UserChatRooms",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_PhotoFileId",
                table: "UserProfiles",
                column: "PhotoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_WallpaperFileId",
                table: "UserProfiles",
                column: "WallpaperFileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AspNetRoleClaims");
            migrationBuilder.DropTable(name: "AspNetUserClaims");
            migrationBuilder.DropTable(name: "AspNetUserLogins");
            migrationBuilder.DropTable(name: "AspNetUserRoles");
            migrationBuilder.DropTable(name: "AspNetUserTokens");
            migrationBuilder.DropTable(name: "AspNetRoles");
            migrationBuilder.DropTable(name: "MessagesReceived");
            migrationBuilder.DropTable(name: "UserChatRooms");
            migrationBuilder.DropTable(name: "MessagesSent");
            migrationBuilder.DropTable(name: "MessageTags");
            migrationBuilder.DropTable(name: "ChatRooms");
            migrationBuilder.DropTable(name: "GroupProfiles");
            migrationBuilder.DropTable(name: "UserProfiles");
            migrationBuilder.DropTable(name: "Files");
            migrationBuilder.DropTable(name: "AspNetUsers");
        }
    }
}
