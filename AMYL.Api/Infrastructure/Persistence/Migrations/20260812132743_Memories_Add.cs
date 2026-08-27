using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMYL.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Memories_Add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UsersId",
                table: "Memories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Memories_UsersId",
                table: "Memories",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Memories_Users_UsersId",
                table: "Memories",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memories_Users_UsersId",
                table: "Memories");

            migrationBuilder.DropIndex(
                name: "IX_Memories_UsersId",
                table: "Memories");

            migrationBuilder.DropColumn(
                name: "UsersId",
                table: "Memories");
        }
    }
}
