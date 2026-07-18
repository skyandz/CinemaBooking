using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaBooking.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueSeatConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BookingDetails_ScheduleId",
                table: "BookingDetails");

            migrationBuilder.AlterColumn<string>(
                name: "SeatNo",
                table: "BookingDetails",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_BookingDetails_ScheduleId_SeatNo",
                table: "BookingDetails",
                columns: new[] { "ScheduleId", "SeatNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BookingDetails_ScheduleId_SeatNo",
                table: "BookingDetails");

            migrationBuilder.AlterColumn<string>(
                name: "SeatNo",
                table: "BookingDetails",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_BookingDetails_ScheduleId",
                table: "BookingDetails",
                column: "ScheduleId");
        }
    }
}
