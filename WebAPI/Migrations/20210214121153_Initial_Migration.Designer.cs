﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebAPI.Models;

namespace WebAPI.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20210214121153_Initial_Migration")]
    partial class Initial_Migration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<long>", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .UseIdentityColumn();

                b.Property<string>("ClaimType")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("ClaimValue")
                    .HasColumnType("nvarchar(450)");

                b.Property<long>("RoleId")
                    .HasColumnType("bigint");

                b.HasKey("Id");

                b.HasIndex("RoleId");

                b.ToTable("AspNetRoleClaims");
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<long>", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .UseIdentityColumn();

                b.Property<string>("ClaimType")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("ClaimValue")
                    .HasColumnType("nvarchar(450)");

                b.Property<long>("UserId")
                    .HasColumnType("bigint");

                b.HasKey("Id");

                b.HasIndex("UserId");

                b.ToTable("AspNetUserClaims");
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<long>", b =>
            {
                b.Property<string>("LoginProvider")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("ProviderKey")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("ProviderDisplayName")
                    .HasColumnType("nvarchar(450)");

                b.Property<long>("UserId")
                    .HasColumnType("bigint");

                b.HasKey("LoginProvider", "ProviderKey");

                b.HasIndex("UserId");

                b.ToTable("AspNetUserLogins");
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<long>", b =>
            {
                b.Property<long>("UserId")
                    .HasColumnType("bigint");

                b.Property<long>("RoleId")
                    .HasColumnType("bigint");

                b.HasKey("UserId", "RoleId");

                b.HasIndex("RoleId");

                b.ToTable("AspNetUserRoles");
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<long>", b =>
            {
                b.Property<long>("UserId")
                    .HasColumnType("bigint");

                b.Property<string>("LoginProvider")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("Name")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("Value")
                    .HasColumnType("nvarchar(450)");

                b.HasKey("UserId", "LoginProvider", "Name");

                b.ToTable("AspNetUserTokens");
            });

            modelBuilder.Entity("WebAPI.Models.ApplicationRole", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bigint")
                    .UseIdentityColumn();

                b.Property<string>("ConcurrencyStamp")
                    .IsConcurrencyToken()
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.Property<string>("Name")
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.Property<string>("NormalizedName")
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.HasKey("Id");

                b.HasIndex("NormalizedName")
                    .IsUnique()
                    .HasDatabaseName("RoleNameIndex")
                    .HasFilter("[NormalizedName] IS NOT NULL");

                b.ToTable("AspNetRoles");
            });

            modelBuilder.Entity("WebAPI.Models.ApplicationUser", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bigint")
                    .UseIdentityColumn();

                b.Property<int>("AccessFailedCount")
                    .HasColumnType("int");

                b.Property<string>("ConcurrencyStamp")
                    .IsConcurrencyToken()
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.Property<string>("Email")
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.Property<bool>("EmailConfirmed")
                    .HasColumnType("bit");

                b.Property<bool>("LockoutEnabled")
                    .HasColumnType("bit");

                b.Property<DateTimeOffset?>("LockoutEnd")
                    .HasColumnType(DataType.Timestamp);

                b.Property<string>("NormalizedEmail")
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.Property<string>("NormalizedUserName")
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.Property<string>("PasswordHash")
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.Property<string>("PhoneNumber")
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.Property<bool>("PhoneNumberConfirmed")
                    .HasColumnType("bit");

                b.Property<string>("SecurityStamp")
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.Property<bool>("TwoFactorEnabled")
                    .HasColumnType("bit");

                b.Property<string>("UserName")
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.HasKey("Id");

                b.HasIndex("NormalizedEmail")
                    .HasDatabaseName("EmailIndex");

                b.HasIndex("NormalizedUserName")
                    .IsUnique()
                    .HasDatabaseName("UserNameIndex")
                    .HasFilter("[NormalizedUserName] IS NOT NULL");

                b.ToTable("AspNetUsers");
            });

            modelBuilder.Entity("WebAPI.Models.ChatRoom", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bigint")
                    .UseIdentityColumn();

                b.Property<DateTime>("DateCreated")
                    .HasColumnType(DataType.DateTime);

                b.Property<long?>("GroupProfileId")
                    .HasColumnType("bigint");

                b.Property<int>("Type")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("GroupProfileId");

                b.ToTable("ChatRooms");
            });

            modelBuilder.Entity("WebAPI.Models.File", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bigint")
                    .UseIdentityColumn();

                b.Property<DateTime?>("DateDeleted")
                    .HasColumnType(DataType.DateTime);

                b.Property<DateTime>("DateUploaded")
                    .HasColumnType(DataType.DateTime);

                b.Property<string>("Name")
                    .HasMaxLength(255)
                    .HasColumnType("nvarchar(255)");

                b.Property<int>("Purpose")
                    .HasColumnType("int");

                b.Property<int>("Size")
                    .HasColumnType("int");

                b.Property<long>("UploaderId")
                    .HasColumnType("bigint");

                b.HasKey("Id");

                b.HasIndex("UploaderId");

                b.ToTable("Files");
            });

            modelBuilder.Entity("WebAPI.Models.GroupProfile", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bigint")
                    .UseIdentityColumn();

                b.Property<string>("About")
                    .HasMaxLength(4095)
                    .HasColumnType(DataType.NVarCharMax);

                b.Property<long>("CreatorId")
                    .HasColumnType("bigint");

                b.Property<DateTime>("DateCreated")
                    .HasColumnType(DataType.DateTime);

                b.Property<DateTime?>("DateDeleted")
                    .HasColumnType(DataType.DateTime);

                b.Property<string>("Groupname")
                    .HasMaxLength(63)
                    .HasColumnType("nvarchar(63)");

                b.Property<long?>("PhotoFileId")
                    .HasColumnType("bigint");

                b.Property<long?>("WallpaperFileId")
                    .HasColumnType("bigint");

                b.HasKey("Id");

                b.HasIndex("CreatorId");

                b.HasIndex("PhotoFileId");

                b.HasIndex("WallpaperFileId");

                b.ToTable("GroupProfiles");
            });

            modelBuilder.Entity("WebAPI.Models.MessageReceived", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bigint")
                    .UseIdentityColumn();

                b.Property<DateTime?>("DateDeleted")
                    .HasColumnType(DataType.DateTime);

                b.Property<DateTime?>("DateRead")
                    .HasColumnType(DataType.DateTime);

                b.Property<DateTime>("DateReceived")
                    .HasColumnType(DataType.DateTime);

                b.Property<DateTime?>("DateStarred")
                    .HasColumnType(DataType.DateTime);

                b.Property<long>("MessageSentId")
                    .HasColumnType("bigint");

                b.Property<int>("Reaction")
                    .HasColumnType("int");

                b.Property<long>("ReceiverId")
                    .HasColumnType("bigint");

                b.HasKey("Id");

                b.HasIndex("MessageSentId");

                b.HasIndex("ReceiverId");

                b.ToTable("MessagesReceived");
            });

            modelBuilder.Entity("WebAPI.Models.MessageSent", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bigint")
                    .UseIdentityColumn();

                b.Property<long?>("AuthorId")
                    .HasColumnType("bigint");

                b.Property<string>("Body")
                    .HasMaxLength(16383)
                    .HasColumnType(DataType.NVarCharMax);

                b.Property<DateTime?>("DateDeleted")
                    .HasColumnType(DataType.DateTime);

                b.Property<DateTime>("DateReceicedByServer")
                    .HasColumnType(DataType.DateTime);

                b.Property<DateTime>("DateSent")
                    .HasColumnType(DataType.DateTime);

                b.Property<DateTime?>("DateStarred")
                    .HasColumnType(DataType.DateTime);

                b.Property<long?>("LinkedId")
                    .HasColumnType("bigint");

                b.Property<long>("MessageTagId")
                    .HasColumnType("bigint");

                b.Property<int>("MessageType")
                    .HasColumnType("int");

                b.Property<long>("SenderId")
                    .HasColumnType("bigint");

                b.HasKey("Id");

                b.HasIndex("AuthorId");

                b.HasIndex("LinkedId");

                b.HasIndex("MessageTagId");

                b.HasIndex("SenderId");

                b.ToTable("MessagesSent");
            });

            modelBuilder.Entity("WebAPI.Models.MessageTag", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bigint")
                    .UseIdentityColumn();

                b.Property<long>("ChatRoomId")
                    .HasColumnType("bigint");

                b.Property<long>("CreatorId")
                    .HasColumnType("bigint");

                b.Property<DateTime>("DateCreated")
                    .HasColumnType(DataType.DateTime);

                b.Property<bool>("IsPrivate")
                    .HasColumnType("bit");

                b.HasKey("Id");

                b.HasIndex("ChatRoomId");

                b.HasIndex("CreatorId");

                b.ToTable("MessageTags");
            });

            modelBuilder.Entity("WebAPI.Models.UserChatRoom", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bigint")
                    .UseIdentityColumn();

                b.Property<long?>("AdderId")
                    .HasColumnType("bigint");

                b.Property<long?>("BlockerId")
                    .HasColumnType("bigint");

                b.Property<long>("ChatRoomId")
                    .HasColumnType("bigint");

                b.Property<DateTime>("DateAdded")
                    .HasColumnType(DataType.DateTime);

                b.Property<DateTime?>("DateBlocked")
                    .HasColumnType(DataType.DateTime);

                b.Property<DateTime?>("DateExited")
                    .HasColumnType(DataType.DateTime);

                b.Property<DateTime?>("DateMuted")
                    .HasColumnType(DataType.DateTime);

                b.Property<TimeSpan?>("MuteDuration")
                    .HasColumnType("time");

                b.Property<long>("UserProfileId")
                    .HasColumnType("bigint");

                b.Property<int>("UserRole")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("AdderId");

                b.HasIndex("BlockerId");

                b.HasIndex("ChatRoomId");

                b.HasIndex("UserProfileId");

                b.ToTable("UserChatRooms");
            });

            modelBuilder.Entity("WebAPI.Models.UserProfile", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bigint")
                    .UseIdentityColumn();

                b.Property<string>("About")
                    .HasMaxLength(4095)
                    .HasColumnType(DataType.NVarCharMax);

                b.Property<string>("Availability")
                    .HasMaxLength(63)
                    .HasColumnType("nvarchar(63)");

                b.Property<DateTime>("DateCreated")
                    .HasColumnType(DataType.DateTime);

                b.Property<DateTime?>("DateDeleted")
                    .HasColumnType(DataType.DateTime);

                b.Property<long?>("PhotoFileId")
                    .HasColumnType("bigint");

                b.Property<long>("UserId")
                    .HasColumnType("bigint");

                b.Property<string>("Username")
                    .HasMaxLength(63)
                    .HasColumnType("nvarchar(63)");

                b.Property<long?>("WallpaperFileId")
                    .HasColumnType("bigint");

                b.HasKey("Id");

                b.HasIndex("PhotoFileId");

                b.HasIndex("UserId");

                b.HasIndex("WallpaperFileId");

                b.ToTable("UserProfiles");
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<long>", b =>
            {
                b.HasOne("WebAPI.Models.ApplicationRole", null)
                    .WithMany()
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<long>", b =>
            {
                b.HasOne("WebAPI.Models.ApplicationUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<long>", b =>
            {
                b.HasOne("WebAPI.Models.ApplicationUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<long>", b =>
            {
                b.HasOne("WebAPI.Models.ApplicationRole", null)
                    .WithMany()
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("WebAPI.Models.ApplicationUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<long>", b =>
            {
                b.HasOne("WebAPI.Models.ApplicationUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("WebAPI.Models.ChatRoom", b =>
            {
                b.HasOne("WebAPI.Models.GroupProfile", "GroupProfile")
                    .WithMany()
                    .HasForeignKey("GroupProfileId");

                b.Navigation("GroupProfile");
            });

            modelBuilder.Entity("WebAPI.Models.File", b =>
            {
                b.HasOne("WebAPI.Models.ApplicationUser", "Uploader")
                    .WithMany()
                    .HasForeignKey("UploaderId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Uploader");
            });

            modelBuilder.Entity("WebAPI.Models.GroupProfile", b =>
            {
                b.HasOne("WebAPI.Models.UserProfile", "Creator")
                    .WithMany()
                    .HasForeignKey("CreatorId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("WebAPI.Models.File", "PhotoFile")
                    .WithMany()
                    .HasForeignKey("PhotoFileId");

                b.HasOne("WebAPI.Models.File", "WallpaperFile")
                    .WithMany()
                    .HasForeignKey("WallpaperFileId");

                b.Navigation("Creator");

                b.Navigation("PhotoFile");

                b.Navigation("WallpaperFile");
            });

            modelBuilder.Entity("WebAPI.Models.MessageReceived", b =>
            {
                b.HasOne("WebAPI.Models.MessageSent", "MessageSent")
                    .WithMany()
                    .HasForeignKey("MessageSentId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("WebAPI.Models.UserChatRoom", "Receiver")
                    .WithMany()
                    .HasForeignKey("ReceiverId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("MessageSent");

                b.Navigation("Receiver");
            });

            modelBuilder.Entity("WebAPI.Models.MessageSent", b =>
            {
                b.HasOne("WebAPI.Models.UserProfile", "Author")
                    .WithMany()
                    .HasForeignKey("AuthorId");

                b.HasOne("WebAPI.Models.MessageSent", "Linked")
                    .WithMany()
                    .HasForeignKey("LinkedId");

                b.HasOne("WebAPI.Models.MessageTag", "MessageTag")
                    .WithMany()
                    .HasForeignKey("MessageTagId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("WebAPI.Models.UserChatRoom", "Sender")
                    .WithMany()
                    .HasForeignKey("SenderId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Author");

                b.Navigation("Linked");

                b.Navigation("MessageTag");

                b.Navigation("Sender");
            });

            modelBuilder.Entity("WebAPI.Models.MessageTag", b =>
            {
                b.HasOne("WebAPI.Models.ChatRoom", "ChatRoom")
                    .WithMany()
                    .HasForeignKey("ChatRoomId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("WebAPI.Models.UserProfile", "Creator")
                    .WithMany()
                    .HasForeignKey("CreatorId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("ChatRoom");

                b.Navigation("Creator");
            });

            modelBuilder.Entity("WebAPI.Models.UserChatRoom", b =>
            {
                b.HasOne("WebAPI.Models.UserProfile", "Adder")
                    .WithMany()
                    .HasForeignKey("AdderId");

                b.HasOne("WebAPI.Models.UserProfile", "Blocker")
                    .WithMany()
                    .HasForeignKey("BlockerId");

                b.HasOne("WebAPI.Models.ChatRoom", "ChatRoom")
                    .WithMany()
                    .HasForeignKey("ChatRoomId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("WebAPI.Models.UserProfile", "UserProfile")
                    .WithMany()
                    .HasForeignKey("UserProfileId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Adder");

                b.Navigation("Blocker");

                b.Navigation("ChatRoom");

                b.Navigation("UserProfile");
            });

            modelBuilder.Entity("WebAPI.Models.UserProfile", b =>
            {
                b.HasOne("WebAPI.Models.File", "PhotoFile")
                    .WithMany()
                    .HasForeignKey("PhotoFileId");

                b.HasOne("WebAPI.Models.ApplicationUser", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("WebAPI.Models.File", "WallpaperFile")
                    .WithMany()
                    .HasForeignKey("WallpaperFileId");

                b.Navigation("PhotoFile");

                b.Navigation("User");

                b.Navigation("WallpaperFile");
            });
#pragma warning restore 612, 618
        }
    }
}
