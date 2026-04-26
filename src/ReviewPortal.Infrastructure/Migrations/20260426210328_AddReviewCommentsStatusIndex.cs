using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReviewPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewCommentsStatusIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ReviewComments_Status",
                table: "ReviewComments",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReviewComments_Status",
                table: "ReviewComments");
        }
    }
}
