﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Messenger.Migrations
{
    public partial class DbInit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    FullName = table.Column<string>(nullable: false),
                    Phone = table.Column<string>(nullable: false),
                    ImageUrl = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<byte[]>(nullable: false),
                    PasswordSalt = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
