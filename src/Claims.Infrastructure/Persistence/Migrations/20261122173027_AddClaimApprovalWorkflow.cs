using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Claims.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimApprovalWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovalRequestedAtUtc",
                table: "Claims",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovalRequestedByUserId",
                table: "Claims",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedSettlementAmount",
                table: "Claims",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DecisionAtUtc",
                table: "Claims",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DecisionByUserId",
                table: "Claims",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProposedSettlementAmount",
                table: "Claims",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Claims",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Claims_ApprovalRequestedByUserId",
                table: "Claims",
                column: "ApprovalRequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_DecisionByUserId",
                table: "Claims",
                column: "DecisionByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_AspNetUsers_ApprovalRequestedByUserId",
                table: "Claims",
                column: "ApprovalRequestedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_AspNetUsers_DecisionByUserId",
                table: "Claims",
                column: "DecisionByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_AspNetUsers_ApprovalRequestedByUserId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_AspNetUsers_DecisionByUserId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_ApprovalRequestedByUserId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_DecisionByUserId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ApprovalRequestedAtUtc",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ApprovalRequestedByUserId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ApprovedSettlementAmount",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DecisionAtUtc",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DecisionByUserId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ProposedSettlementAmount",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Claims");
        }
    }
}
