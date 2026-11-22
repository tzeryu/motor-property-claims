using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Claims.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SubmittedByUserId",
                table: "Claims",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Claims_SubmittedByUserId",
                table: "Claims",
                column: "SubmittedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_AspNetUsers_SubmittedByUserId",
                table: "Claims",
                column: "SubmittedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_AspNetUsers_SubmittedByUserId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_SubmittedByUserId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "Claims");
        }
    }
}
