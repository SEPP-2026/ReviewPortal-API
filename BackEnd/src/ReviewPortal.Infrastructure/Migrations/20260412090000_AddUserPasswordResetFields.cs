using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ReviewPortal.Infrastructure.Data;

#nullable disable

namespace ReviewPortal.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260412090000_AddUserPasswordResetFields")]
public partial class AddUserPasswordResetFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "PasswordResetTokenExpiryUtc",
            table: "Users",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PasswordResetTokenHash",
            table: "Users",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PasswordResetTokenExpiryUtc",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "PasswordResetTokenHash",
            table: "Users");
    }
}
