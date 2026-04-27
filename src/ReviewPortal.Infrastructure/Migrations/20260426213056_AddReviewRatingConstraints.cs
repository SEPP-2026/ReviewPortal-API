using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReviewPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewRatingConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Reviews_AfterSalesRating_Range",
                table: "Reviews",
                sql: "[AfterSalesRating] BETWEEN 1 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reviews_CustomerServiceRating_Range",
                table: "Reviews",
                sql: "[CustomerServiceRating] BETWEEN 1 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reviews_EquipmentRating_Range",
                table: "Reviews",
                sql: "[EquipmentRating] BETWEEN 1 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reviews_TechnicalSupportRating_Range",
                table: "Reviews",
                sql: "[TechnicalSupportRating] BETWEEN 1 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reviews_ValueForMoneyRating_Range",
                table: "Reviews",
                sql: "[ValueForMoneyRating] BETWEEN 1 AND 5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Reviews_AfterSalesRating_Range",
                table: "Reviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Reviews_CustomerServiceRating_Range",
                table: "Reviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Reviews_EquipmentRating_Range",
                table: "Reviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Reviews_TechnicalSupportRating_Range",
                table: "Reviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Reviews_ValueForMoneyRating_Range",
                table: "Reviews");
        }
    }
}
