using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devclinic.MedicalRecords.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxIdexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_EventId",
                table: "OutboxMessages",
                column: "EventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_EventId",
                table: "OutboxMessages");
        }
    }
}
