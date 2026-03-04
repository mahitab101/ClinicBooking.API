using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicBooking.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "DoctorSchedule",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "DoctorSchedule",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DoctorSchedule",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "DoctorSchedule",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "DoctorSchedule",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DoctorSchedule");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "DoctorSchedule");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DoctorSchedule");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "DoctorSchedule");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "DoctorSchedule");
        }
    }
}
