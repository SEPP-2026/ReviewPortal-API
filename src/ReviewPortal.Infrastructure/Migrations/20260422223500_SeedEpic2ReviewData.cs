using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ReviewPortal.Infrastructure.Data;

#nullable disable

namespace ReviewPortal.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260422223500_SeedEpic2ReviewData")]
public partial class SeedEpic2ReviewData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            SET IDENTITY_INSERT dbo.Reviews ON;

            MERGE dbo.Reviews AS target
            USING (VALUES
                (4001, 2001, NULL, N'Alice Morgan', N'alice.morgan@example.com', N'The cement mixer arrived clean, ran smoothly all weekend, and the collection handover was very straightforward.', 5, 4, 4, 5, 4, 4.40, N'Approved', NULL, CAST('2026-04-09T09:30:00' AS datetime2)),
                (4002, 2001, NULL, N'Ben Carter', N'ben.carter@example.com', N'Reliable mixer for a patio base. Staff explained the transformer setup clearly and the hire price felt fair.', 4, 4, 4, 5, 4, 4.20, N'Approved', NULL, CAST('2026-04-11T14:15:00' AS datetime2)),
                (4003, 2002, NULL, N'Priya Shah', N'priya.shah@example.com', N'The concrete saw cut through old paving cleanly and the team gave helpful safety guidance before we left.', 5, 5, 5, 5, 4, 4.80, N'Approved', NULL, CAST('2026-04-10T10:45:00' AS datetime2)),
                (4004, 2002, NULL, N'Daniel Hughes', N'daniel.hughes@example.com', N'Powerful saw with a sharp blade and clear instructions. Dust control advice from the counter team was useful.', 5, 4, 5, 5, 5, 4.80, N'Approved', NULL, CAST('2026-04-13T11:20:00' AS datetime2)),
                (4005, 2005, NULL, N'Emma Wilson', N'emma.wilson@example.com', N'The rotavator handled compacted soil better than expected and was ready for collection at the promised time.', 4, 4, 4, 4, 4, 4.00, N'Approved', NULL, CAST('2026-04-12T08:10:00' AS datetime2)),
                (4006, 2005, NULL, N'Liam Foster', N'liam.foster@example.com', N'Good value for preparing a vegetable plot. The machine worked well, though it took a little practice to control.', 4, 4, 3, 4, 4, 3.80, N'Approved', NULL, CAST('2026-04-14T16:40:00' AS datetime2)),
                (4007, 2007, NULL, N'Olivia Green', N'olivia.green@example.com', N'The dehumidifier dried fresh plaster quickly and the branch team helped us choose the right size for the room.', 5, 5, 4, 5, 4, 4.60, N'Approved', NULL, CAST('2026-04-15T09:05:00' AS datetime2)),
                (4008, 2011, NULL, N'Noah Brooks', N'noah.brooks@example.com', N'The SDS drill had plenty of power, but I want the team to check whether the bit wear charge was applied correctly.', 4, 3, 4, 3, 3, 3.40, N'Pending', NULL, CAST('2026-04-16T13:25:00' AS datetime2)),
                (4009, 2020, NULL, N'Sophia Turner', N'sophia.turner@example.com', N'The wallpaper steamer worked well overall, but this review includes job-specific details that need staff moderation.', 4, 4, 3, 4, 4, 3.80, N'Pending', NULL, CAST('2026-04-17T15:55:00' AS datetime2))
            ) AS source (Id, ToolId, UserId, ReviewerName, ReviewerEmail, ReviewText, EquipmentRating, CustomerServiceRating, TechnicalSupportRating, AfterSalesRating, ValueForMoneyRating, OverallRating, Status, RejectionReason, CreatedDate)
            ON target.Id = source.Id
            WHEN MATCHED THEN
                UPDATE SET
                    ToolId = source.ToolId,
                    UserId = source.UserId,
                    ReviewerName = source.ReviewerName,
                    ReviewerEmail = source.ReviewerEmail,
                    ReviewText = source.ReviewText,
                    EquipmentRating = source.EquipmentRating,
                    CustomerServiceRating = source.CustomerServiceRating,
                    TechnicalSupportRating = source.TechnicalSupportRating,
                    AfterSalesRating = source.AfterSalesRating,
                    ValueForMoneyRating = source.ValueForMoneyRating,
                    OverallRating = source.OverallRating,
                    Status = source.Status,
                    RejectionReason = source.RejectionReason,
                    CreatedDate = source.CreatedDate
            WHEN NOT MATCHED BY TARGET THEN
                INSERT (Id, ToolId, UserId, ReviewerName, ReviewerEmail, ReviewText, EquipmentRating, CustomerServiceRating, TechnicalSupportRating, AfterSalesRating, ValueForMoneyRating, OverallRating, Status, RejectionReason, CreatedDate)
                VALUES (source.Id, source.ToolId, source.UserId, source.ReviewerName, source.ReviewerEmail, source.ReviewText, source.EquipmentRating, source.CustomerServiceRating, source.TechnicalSupportRating, source.AfterSalesRating, source.ValueForMoneyRating, source.OverallRating, source.Status, source.RejectionReason, source.CreatedDate);

            SET IDENTITY_INSERT dbo.Reviews OFF;

            MERGE dbo.Tools AS target
            USING (VALUES
                (2001, CAST(4.30 AS decimal(3,2)), 2),
                (2002, CAST(4.80 AS decimal(3,2)), 2),
                (2003, CAST(NULL AS decimal(3,2)), 0),
                (2005, CAST(3.90 AS decimal(3,2)), 2),
                (2007, CAST(4.60 AS decimal(3,2)), 1),
                (2009, CAST(NULL AS decimal(3,2)), 0),
                (2011, CAST(NULL AS decimal(3,2)), 0)
            ) AS source (Id, OverallRating, ReviewCount)
            ON target.Id = source.Id
            WHEN MATCHED THEN
                UPDATE SET
                    OverallRating = source.OverallRating,
                    ReviewCount = source.ReviewCount,
                    UpdatedDate = CAST('2026-04-17T16:00:00' AS datetime2);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM dbo.Reviews
            WHERE Id BETWEEN 4001 AND 4009;

            MERGE dbo.Tools AS target
            USING (VALUES
                (2001, CAST(4.30 AS decimal(3,2)), 2),
                (2002, CAST(4.80 AS decimal(3,2)), 1),
                (2003, CAST(4.20 AS decimal(3,2)), 1),
                (2005, CAST(3.90 AS decimal(3,2)), 2),
                (2007, CAST(4.60 AS decimal(3,2)), 1),
                (2009, CAST(4.40 AS decimal(3,2)), 1),
                (2011, CAST(4.00 AS decimal(3,2)), 1)
            ) AS source (Id, OverallRating, ReviewCount)
            ON target.Id = source.Id
            WHEN MATCHED THEN
                UPDATE SET
                    OverallRating = source.OverallRating,
                    ReviewCount = source.ReviewCount,
                    UpdatedDate = CAST('2026-04-08T09:00:00' AS datetime2);
            """);
    }
}
