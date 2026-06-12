using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devclinic.MedicalRecords.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MedicalRecordIndexesProjection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicalRecordIndexes",
                columns: table => new
                {
                    MedicalRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRecordIndexes", x => x.MedicalRecordId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecordIndexes_PatientId",
                table: "MedicalRecordIndexes",
                column: "PatientId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalRecordIndexes");
        }
    }
}
