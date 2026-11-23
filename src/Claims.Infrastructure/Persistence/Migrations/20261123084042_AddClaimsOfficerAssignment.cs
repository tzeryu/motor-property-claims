using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Claims.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimsOfficerAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AssignedAtUtc",
                table: "Claims",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedOfficerUserId",
                table: "Claims",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Claims",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Claims_AssignedOfficerUserId",
                table: "Claims",
                column: "AssignedOfficerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_AspNetUsers_AssignedOfficerUserId",
                table: "Claims",
                column: "AssignedOfficerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_AspNetUsers_AssignedOfficerUserId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_AssignedOfficerUserId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AssignedAtUtc",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AssignedOfficerUserId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Claims");
        }
    }
}
