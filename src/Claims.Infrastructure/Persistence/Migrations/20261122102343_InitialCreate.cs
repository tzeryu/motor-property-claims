using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Claims.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ClaimantName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ClaimantEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    ClaimantPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PolicyNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PolicyholderName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CoverageStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CoverageEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    InsuredAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IncidentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IncidentLocation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IncidentDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MotorClaimDetails",
                columns: table => new
                {
                    ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleRegistrationNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VehicleMake = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VehicleModel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotorClaimDetails", x => x.ClaimId);
                    table.ForeignKey(
                        name: "FK_MotorClaimDetails_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyClaimDetails",
                columns: table => new
                {
                    ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropertyAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PropertyType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DamageDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyClaimDetails", x => x.ClaimId);
                    table.ForeignKey(
                        name: "FK_PropertyClaimDetails_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_ClaimNumber",
                table: "Claims",
                column: "ClaimNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Claims_Status",
                table: "Claims",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MotorClaimDetails");

            migrationBuilder.DropTable(
                name: "PropertyClaimDetails");

            migrationBuilder.DropTable(
                name: "Claims");
        }
    }
}
