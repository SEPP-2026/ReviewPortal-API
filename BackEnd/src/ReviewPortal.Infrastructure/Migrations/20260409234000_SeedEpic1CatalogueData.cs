using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ReviewPortal.Infrastructure.Data;

#nullable disable

namespace ReviewPortal.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260409234000_SeedEpic1CatalogueData")]
public partial class SeedEpic1CatalogueData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            SET IDENTITY_INSERT dbo.Categories ON;

            MERGE dbo.Categories AS target
            USING (VALUES
                (1001, N'Building & Construction', N'Heavy-duty site equipment for mixing, cutting, and structural work.', N'https://cdn.reviewportal.local/categories/building-construction.jpg'),
                (1002, N'Cleaning & Maintenance', N'Machines for site clean-up, spill control, and deep cleaning.', N'https://cdn.reviewportal.local/categories/cleaning-maintenance.jpg'),
                (1003, N'Garden & Landscaping', N'Outdoor equipment for clearing, cutting, and ground preparation.', N'https://cdn.reviewportal.local/categories/garden-landscaping.jpg'),
                (1004, N'Electrical & Heating', N'Temporary power, drying, and environmental control equipment.', N'https://cdn.reviewportal.local/categories/electrical-heating.jpg'),
                (1005, N'Access & Lifting', N'Safe access platforms and lifting aids for working at height.', N'https://cdn.reviewportal.local/categories/access-lifting.jpg'),
                (1006, N'Breaking & Drilling', N'Core drilling, demolition, and concrete breaking equipment.', N'https://cdn.reviewportal.local/categories/breaking-drilling.jpg')
            ) AS source (Id, Name, Description, ImageUrl)
            ON target.Id = source.Id
            WHEN MATCHED THEN
                UPDATE SET Name = source.Name, Description = source.Description, ImageUrl = source.ImageUrl
            WHEN NOT MATCHED BY TARGET THEN
                INSERT (Id, Name, Description, ImageUrl)
                VALUES (source.Id, source.Name, source.Description, source.ImageUrl);

            SET IDENTITY_INSERT dbo.Categories OFF;

            SET IDENTITY_INSERT dbo.Tools ON;

            MERGE dbo.Tools AS target
            USING (VALUES
                (2001, 1001, N'Cement Mixer 110V', N'Site-ready 110V cement mixer suitable for concrete bases, fence posts, and medium-sized construction jobs.', 12.00, 45.00, 150.00, N'Supplied with stand and drum. A 110V transformer is required on site.', 1, 80.00, 1, 4.30, 2, CAST('2026-04-06T08:00:00' AS datetime2), CAST('2026-04-08T09:00:00' AS datetime2)),
                (2002, 1001, N'Concrete Saw 350mm', N'Petrol concrete saw for cutting paving, concrete slabs, kerbs, and asphalt with a deep clean finish.', 18.00, 65.00, 220.00, N'Blade wear is chargeable. Use wet cutting where dust control is required.', 1, 120.00, 1, 4.80, 1, CAST('2026-04-06T08:10:00' AS datetime2), CAST('2026-04-08T09:05:00' AS datetime2)),
                (2003, 1002, N'Petrol Pressure Washer', N'High-output pressure washer for site vehicles, paving, shuttering, and heavy ground-in dirt.', 14.00, 52.00, 175.00, N'Best performance with a hose-fed water supply and outdoor use.', 0, NULL, 1, 4.20, 1, CAST('2026-04-06T08:20:00' AS datetime2), CAST('2026-04-08T09:10:00' AS datetime2)),
                (2004, 1002, N'Industrial Wet Vacuum', N'Large-capacity wet and dry vacuum for flood clean-up, dust extraction, and workshop debris.', 9.00, 32.00, 110.00, N'Collection bags and filters are available separately.', 0, NULL, 1, NULL, 0, CAST('2026-04-06T08:30:00' AS datetime2), CAST('2026-04-08T09:15:00' AS datetime2)),
                (2005, 1003, N'Heavy Duty Rotavator', N'Petrol rotavator for breaking up compacted soil, preparing borders, and turning over allotment ground.', 16.00, 58.00, 195.00, N'Sturdy boots and eye protection are recommended during operation.', 1, 90.00, 1, 3.90, 2, CAST('2026-04-06T08:40:00' AS datetime2), CAST('2026-04-08T09:20:00' AS datetime2)),
                (2006, 1003, N'Wood Chipper 6in', N'Towable wood chipper for branches, hedge cuttings, and large garden clearance work.', 24.00, 82.00, 275.00, N'Towing vehicle must be rated correctly. Ear defenders are recommended.', 1, 150.00, 1, NULL, 0, CAST('2026-04-06T08:50:00' AS datetime2), CAST('2026-04-08T09:25:00' AS datetime2)),
                (2007, 1004, N'50L Dehumidifier', N'Commercial dehumidifier for drying plaster, damp spaces, and post-leak recovery jobs.', 11.00, 40.00, 135.00, N'Allow airflow clearance around the unit for best drying performance.', 0, NULL, 1, 4.60, 1, CAST('2026-04-06T09:00:00' AS datetime2), CAST('2026-04-08T09:30:00' AS datetime2)),
                (2008, 1004, N'PAT Tester Kit', N'Portable appliance testing kit with leads and labels for site compliance checks and scheduled inspections.', 13.00, 48.00, 160.00, N'Calibration certificate is included with each hire.', 1, 60.00, 1, NULL, 0, CAST('2026-04-06T09:10:00' AS datetime2), CAST('2026-04-08T09:35:00' AS datetime2)),
                (2009, 1005, N'Platform Ladder 3.6m', N'Professional platform ladder with guard rails for decorating, electrical, and maintenance work.', 10.00, 34.00, 112.00, N'Inspect feet and stabilisers before use on smooth surfaces.', 0, NULL, 1, 4.40, 1, CAST('2026-04-06T09:20:00' AS datetime2), CAST('2026-04-08T09:40:00' AS datetime2)),
                (2010, 1005, N'Material Hoist', N'Compact hoist for lifting plasterboard, ductwork, and awkward building materials into position.', 20.00, 74.00, 248.00, N'Assembly takes two people. Delivery is recommended for upper-floor work.', 1, 140.00, 1, NULL, 0, CAST('2026-04-06T09:30:00' AS datetime2), CAST('2026-04-08T09:45:00' AS datetime2)),
                (2011, 1006, N'SDS Max Drill', N'Heavy-duty SDS Max rotary hammer for concrete drilling, chiselling, and anchor preparation.', 15.00, 54.00, 180.00, N'Bits are hired separately and charged according to wear.', 0, NULL, 1, 4.00, 1, CAST('2026-04-06T09:40:00' AS datetime2), CAST('2026-04-08T09:50:00' AS datetime2)),
                (2012, 1006, N'Hydraulic Breaker', N'Powerful breaker for slabs, reinforced concrete, and tough demolition work on site.', 22.00, 79.00, 265.00, N'Use with suitable hydraulic pack and trained operator guidance.', 1, 180.00, 1, NULL, 0, CAST('2026-04-06T09:50:00' AS datetime2), CAST('2026-04-08T09:55:00' AS datetime2)),
                (2013, 1001, N'Floor Sander 240V', N'Heavy-duty floor sander for boards, site cabins, and timber surface preparation before finishing.', 17.00, 63.00, 210.00, N'Dust bags are available separately. Use suitable hearing protection.', 1, 100.00, 1, NULL, 0, CAST('2026-04-06T10:00:00' AS datetime2), CAST('2026-04-08T10:00:00' AS datetime2)),
                (2014, 1001, N'Acrow Prop No. 2', N'Adjustable steel support prop for temporary structural support during building and renovation work.', 6.00, 20.00, 65.00, N'Load calculations must be confirmed by a competent person before use.', 0, NULL, 1, NULL, 0, CAST('2026-04-06T10:10:00' AS datetime2), CAST('2026-04-08T10:05:00' AS datetime2)),
                (2015, 1002, N'Carpet Cleaner Pro', N'Commercial carpet cleaner for offices, rentals, and site accommodation deep-clean work.', 10.00, 38.00, 125.00, N'Cleaning solution is sold separately and should match the surface type.', 0, NULL, 1, NULL, 0, CAST('2026-04-06T10:20:00' AS datetime2), CAST('2026-04-08T10:10:00' AS datetime2)),
                (2016, 1002, N'Floor Scrubber Dryer', N'Walk-behind scrubber dryer for warehouses, workshops, and large hard-floor areas.', 18.00, 68.00, 230.00, N'Check battery charge before longer cleaning sessions.', 1, 120.00, 1, NULL, 0, CAST('2026-04-06T10:30:00' AS datetime2), CAST('2026-04-08T10:15:00' AS datetime2)),
                (2017, 1003, N'Hedge Trimmer Long Reach', N'Long-reach hedge trimmer for tall hedges, boundary work, and awkward landscaping cuts.', 12.00, 44.00, 145.00, N'Eye protection and gloves are recommended during operation.', 0, NULL, 1, NULL, 0, CAST('2026-04-06T10:40:00' AS datetime2), CAST('2026-04-08T10:20:00' AS datetime2)),
                (2018, 1003, N'Turf Cutter', N'Petrol turf cutter for lifting lawns cleanly before relaying, landscaping, or groundworks.', 19.00, 72.00, 240.00, N'Best used after light watering if ground conditions are very dry.', 1, 110.00, 1, NULL, 0, CAST('2026-04-06T10:50:00' AS datetime2), CAST('2026-04-08T10:25:00' AS datetime2))
            ) AS source (Id, CategoryId, Name, Description, HourlyRate, DailyRate, WeeklyRate, SpecialNotes, DepositRequired, DepositAmount, IsActive, OverallRating, ReviewCount, CreatedDate, UpdatedDate)
            ON target.Id = source.Id
            WHEN MATCHED THEN
                UPDATE SET
                    CategoryId = source.CategoryId,
                    Name = source.Name,
                    Description = source.Description,
                    HourlyRate = source.HourlyRate,
                    DailyRate = source.DailyRate,
                    WeeklyRate = source.WeeklyRate,
                    SpecialNotes = source.SpecialNotes,
                    DepositRequired = source.DepositRequired,
                    DepositAmount = source.DepositAmount,
                    IsActive = source.IsActive,
                    OverallRating = source.OverallRating,
                    ReviewCount = source.ReviewCount,
                    CreatedDate = source.CreatedDate,
                    UpdatedDate = source.UpdatedDate
            WHEN NOT MATCHED BY TARGET THEN
                INSERT (Id, CategoryId, Name, Description, HourlyRate, DailyRate, WeeklyRate, SpecialNotes, DepositRequired, DepositAmount, IsActive, OverallRating, ReviewCount, CreatedDate, UpdatedDate)
                VALUES (source.Id, source.CategoryId, source.Name, source.Description, source.HourlyRate, source.DailyRate, source.WeeklyRate, source.SpecialNotes, source.DepositRequired, source.DepositAmount, source.IsActive, source.OverallRating, source.ReviewCount, source.CreatedDate, source.UpdatedDate);

            SET IDENTITY_INSERT dbo.Tools OFF;

            SET IDENTITY_INSERT dbo.ToolImages ON;

            MERGE dbo.ToolImages AS target
            USING (VALUES
                (3001, 2001, N'https://cdn.reviewportal.local/tools/cement-mixer-1.jpg', 1, CAST('2026-04-06T10:00:00' AS datetime2)),
                (3002, 2001, N'https://cdn.reviewportal.local/tools/cement-mixer-2.jpg', 2, CAST('2026-04-06T10:01:00' AS datetime2)),
                (3003, 2002, N'https://cdn.reviewportal.local/tools/concrete-saw-1.jpg', 1, CAST('2026-04-06T10:02:00' AS datetime2)),
                (3004, 2002, N'https://cdn.reviewportal.local/tools/concrete-saw-2.jpg', 2, CAST('2026-04-06T10:03:00' AS datetime2)),
                (3005, 2003, N'https://cdn.reviewportal.local/tools/pressure-washer-1.jpg', 1, CAST('2026-04-06T10:04:00' AS datetime2)),
                (3006, 2003, N'https://cdn.reviewportal.local/tools/pressure-washer-2.jpg', 2, CAST('2026-04-06T10:05:00' AS datetime2)),
                (3007, 2004, N'https://cdn.reviewportal.local/tools/wet-vacuum-1.jpg', 1, CAST('2026-04-06T10:06:00' AS datetime2)),
                (3008, 2004, N'https://cdn.reviewportal.local/tools/wet-vacuum-2.jpg', 2, CAST('2026-04-06T10:07:00' AS datetime2)),
                (3009, 2005, N'https://cdn.reviewportal.local/tools/rotavator-1.jpg', 1, CAST('2026-04-06T10:08:00' AS datetime2)),
                (3010, 2005, N'https://cdn.reviewportal.local/tools/rotavator-2.jpg', 2, CAST('2026-04-06T10:09:00' AS datetime2)),
                (3011, 2006, N'https://cdn.reviewportal.local/tools/wood-chipper-1.jpg', 1, CAST('2026-04-06T10:10:00' AS datetime2)),
                (3012, 2006, N'https://cdn.reviewportal.local/tools/wood-chipper-2.jpg', 2, CAST('2026-04-06T10:11:00' AS datetime2)),
                (3013, 2007, N'https://cdn.reviewportal.local/tools/dehumidifier-1.jpg', 1, CAST('2026-04-06T10:12:00' AS datetime2)),
                (3014, 2007, N'https://cdn.reviewportal.local/tools/dehumidifier-2.jpg', 2, CAST('2026-04-06T10:13:00' AS datetime2)),
                (3015, 2008, N'https://cdn.reviewportal.local/tools/pat-tester-1.jpg', 1, CAST('2026-04-06T10:14:00' AS datetime2)),
                (3016, 2008, N'https://cdn.reviewportal.local/tools/pat-tester-2.jpg', 2, CAST('2026-04-06T10:15:00' AS datetime2)),
                (3017, 2009, N'https://cdn.reviewportal.local/tools/platform-ladder-1.jpg', 1, CAST('2026-04-06T10:16:00' AS datetime2)),
                (3018, 2009, N'https://cdn.reviewportal.local/tools/platform-ladder-2.jpg', 2, CAST('2026-04-06T10:17:00' AS datetime2)),
                (3019, 2010, N'https://cdn.reviewportal.local/tools/material-hoist-1.jpg', 1, CAST('2026-04-06T10:18:00' AS datetime2)),
                (3020, 2010, N'https://cdn.reviewportal.local/tools/material-hoist-2.jpg', 2, CAST('2026-04-06T10:19:00' AS datetime2)),
                (3021, 2011, N'https://cdn.reviewportal.local/tools/sds-max-drill-1.jpg', 1, CAST('2026-04-06T10:20:00' AS datetime2)),
                (3022, 2011, N'https://cdn.reviewportal.local/tools/sds-max-drill-2.jpg', 2, CAST('2026-04-06T10:21:00' AS datetime2)),
                (3023, 2012, N'https://cdn.reviewportal.local/tools/hydraulic-breaker-1.jpg', 1, CAST('2026-04-06T10:22:00' AS datetime2)),
                (3024, 2012, N'https://cdn.reviewportal.local/tools/hydraulic-breaker-2.jpg', 2, CAST('2026-04-06T10:23:00' AS datetime2)),
                (3025, 2013, N'https://cdn.reviewportal.local/tools/floor-sander-1.jpg', 1, CAST('2026-04-06T10:24:00' AS datetime2)),
                (3026, 2013, N'https://cdn.reviewportal.local/tools/floor-sander-2.jpg', 2, CAST('2026-04-06T10:25:00' AS datetime2)),
                (3027, 2014, N'https://cdn.reviewportal.local/tools/acrow-prop-1.jpg', 1, CAST('2026-04-06T10:26:00' AS datetime2)),
                (3028, 2014, N'https://cdn.reviewportal.local/tools/acrow-prop-2.jpg', 2, CAST('2026-04-06T10:27:00' AS datetime2)),
                (3029, 2015, N'https://cdn.reviewportal.local/tools/carpet-cleaner-1.jpg', 1, CAST('2026-04-06T10:28:00' AS datetime2)),
                (3030, 2015, N'https://cdn.reviewportal.local/tools/carpet-cleaner-2.jpg', 2, CAST('2026-04-06T10:29:00' AS datetime2)),
                (3031, 2016, N'https://cdn.reviewportal.local/tools/floor-scrubber-1.jpg', 1, CAST('2026-04-06T10:30:00' AS datetime2)),
                (3032, 2016, N'https://cdn.reviewportal.local/tools/floor-scrubber-2.jpg', 2, CAST('2026-04-06T10:31:00' AS datetime2)),
                (3033, 2017, N'https://cdn.reviewportal.local/tools/hedge-trimmer-1.jpg', 1, CAST('2026-04-06T10:32:00' AS datetime2)),
                (3034, 2017, N'https://cdn.reviewportal.local/tools/hedge-trimmer-2.jpg', 2, CAST('2026-04-06T10:33:00' AS datetime2)),
                (3035, 2018, N'https://cdn.reviewportal.local/tools/turf-cutter-1.jpg', 1, CAST('2026-04-06T10:34:00' AS datetime2)),
                (3036, 2018, N'https://cdn.reviewportal.local/tools/turf-cutter-2.jpg', 2, CAST('2026-04-06T10:35:00' AS datetime2))
            ) AS source (Id, ToolId, ImageUrl, DisplayOrder, UploadedDate)
            ON target.Id = source.Id
            WHEN MATCHED THEN
                UPDATE SET ToolId = source.ToolId, ImageUrl = source.ImageUrl, DisplayOrder = source.DisplayOrder, UploadedDate = source.UploadedDate
            WHEN NOT MATCHED BY TARGET THEN
                INSERT (Id, ToolId, ImageUrl, DisplayOrder, UploadedDate)
                VALUES (source.Id, source.ToolId, source.ImageUrl, source.DisplayOrder, source.UploadedDate);

            SET IDENTITY_INSERT dbo.ToolImages OFF;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM dbo.ToolImages
            WHERE Id BETWEEN 3001 AND 3036;

            DELETE tools
            FROM dbo.Tools AS tools
            WHERE tools.Id BETWEEN 2001 AND 2018
              AND NOT EXISTS (SELECT 1 FROM dbo.Reviews AS reviews WHERE reviews.ToolId = tools.Id);

            DELETE categories
            FROM dbo.Categories AS categories
            WHERE categories.Id BETWEEN 1001 AND 1006
              AND NOT EXISTS (SELECT 1 FROM dbo.Tools AS tools WHERE tools.CategoryId = categories.Id);
            """);
    }
}
