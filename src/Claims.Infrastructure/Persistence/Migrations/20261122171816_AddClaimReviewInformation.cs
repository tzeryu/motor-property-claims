using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Claims.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimReviewInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalInformationRequest",
                table: "Claims",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AdditionalInformationRequestedAtUtc",
                table: "Claims",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AdditionalInformationRequestedByUserId",
                table: "Claims",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AdditionalInformationRespondedAtUtc",
                table: "Claims",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalInformationResponse",
                table: "Claims",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastReviewedAtUtc",
                table: "Claims",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastReviewedByUserId",
                table: "Claims",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Claims_AdditionalInformationRequestedByUserId",
                table: "Claims",
                column: "AdditionalInformationRequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_LastReviewedByUserId",
                table: "Claims",
                column: "LastReviewedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_AspNetUsers_AdditionalInformationRequestedByUserId",
                table: "Claims",
                column: "AdditionalInformationRequestedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_AspNetUsers_LastReviewedByUserId",
                table: "Claims",
                column: "LastReviewedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_AspNetUsers_AdditionalInformationRequestedByUserId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_AspNetUsers_LastReviewedByUserId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_AdditionalInformationRequestedByUserId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_LastReviewedByUserId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AdditionalInformationRequest",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AdditionalInformationRequestedAtUtc",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AdditionalInformationRequestedByUserId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AdditionalInformationRespondedAtUtc",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AdditionalInformationResponse",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "LastReviewedAtUtc",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "LastReviewedByUserId",
                table: "Claims");
        }
    }
}
