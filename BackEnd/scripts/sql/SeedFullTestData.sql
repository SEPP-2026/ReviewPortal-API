/*
  ReviewPortal full relational test data

  Seeds all business tables with linked development data:
  - Users
  - Categories
  - Tools
  - ToolImages
  - Reviews
  - ReviewComments
  - CompanyResponses

  Existing test user credentials:
  - customer.test@reviewportal.local / Customer123!
  - admin.test@reviewportal.local / Admin123!
  - moderator.test@reviewportal.local / Moderator123!

  Intended for development and local testing on a fresh or disposable database.
  Safe to run multiple times. Re-running refreshes the seeded records to the
  canonical development state and clears any stored password reset tokens when
  those columns exist.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @HasPasswordResetColumns bit = CASE
    WHEN COL_LENGTH('dbo.Users', 'PasswordResetTokenHash') IS NOT NULL
     AND COL_LENGTH('dbo.Users', 'PasswordResetTokenExpiryUtc') IS NOT NULL
    THEN 1
    ELSE 0
END;

SET IDENTITY_INSERT dbo.Users ON;

IF @HasPasswordResetColumns = 1
BEGIN
    MERGE dbo.Users AS target
    USING (VALUES
        (1, N'Test Customer', N'customer.test@reviewportal.local', N'AQAAAAIAAYagAAAAEA0G/BQ4WPdNpKqAnK0Mam0BNoG560l4GFfcdOa4Xps8ZYyGiiH6yxH5EgC1llfMqQ==', CAST(NULL AS nvarchar(256)), CAST(NULL AS datetime2), N'Customer', CAST('2026-04-06T00:00:00' AS datetime2)),
        (2, N'Test Admin', N'admin.test@reviewportal.local', N'AQAAAAIAAYagAAAAEOqaZaaWb29MnbnePdQNWq+wQO66MHMXfZh0ouwcBZjORmC3mllgI0zebkL9iOSFqQ==', CAST(NULL AS nvarchar(256)), CAST(NULL AS datetime2), N'Admin', CAST('2026-04-06T00:00:00' AS datetime2)),
        (3, N'Test Moderator', N'moderator.test@reviewportal.local', N'AQAAAAIAAYagAAAAEMnrqmN8EAAHiVorrGMGoFopVvlvMBVCFw0pvQz9mgWzxZrB1RGNwUsqv+n41Wss/g==', CAST(NULL AS nvarchar(256)), CAST(NULL AS datetime2), N'Moderator', CAST('2026-04-06T00:00:00' AS datetime2))
    ) AS source (Id, Name, Email, PasswordHash, PasswordResetTokenHash, PasswordResetTokenExpiryUtc, Role, CreatedDate)
    ON target.Id = source.Id
    WHEN MATCHED THEN
        UPDATE SET
            Name = source.Name,
            Email = source.Email,
            PasswordHash = source.PasswordHash,
            PasswordResetTokenHash = source.PasswordResetTokenHash,
            PasswordResetTokenExpiryUtc = source.PasswordResetTokenExpiryUtc,
            Role = source.Role,
            CreatedDate = source.CreatedDate
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (Id, Name, Email, PasswordHash, PasswordResetTokenHash, PasswordResetTokenExpiryUtc, Role, CreatedDate)
        VALUES (source.Id, source.Name, source.Email, source.PasswordHash, source.PasswordResetTokenHash, source.PasswordResetTokenExpiryUtc, source.Role, source.CreatedDate);
END
ELSE
BEGIN
    MERGE dbo.Users AS target
    USING (VALUES
        (1, N'Test Customer', N'customer.test@reviewportal.local', N'AQAAAAIAAYagAAAAEA0G/BQ4WPdNpKqAnK0Mam0BNoG560l4GFfcdOa4Xps8ZYyGiiH6yxH5EgC1llfMqQ==', N'Customer', CAST('2026-04-06T00:00:00' AS datetime2)),
        (2, N'Test Admin', N'admin.test@reviewportal.local', N'AQAAAAIAAYagAAAAEOqaZaaWb29MnbnePdQNWq+wQO66MHMXfZh0ouwcBZjORmC3mllgI0zebkL9iOSFqQ==', N'Admin', CAST('2026-04-06T00:00:00' AS datetime2)),
        (3, N'Test Moderator', N'moderator.test@reviewportal.local', N'AQAAAAIAAYagAAAAEMnrqmN8EAAHiVorrGMGoFopVvlvMBVCFw0pvQz9mgWzxZrB1RGNwUsqv+n41Wss/g==', N'Moderator', CAST('2026-04-06T00:00:00' AS datetime2))
    ) AS source (Id, Name, Email, PasswordHash, Role, CreatedDate)
    ON target.Id = source.Id
    WHEN MATCHED THEN
        UPDATE SET
            Name = source.Name,
            Email = source.Email,
            PasswordHash = source.PasswordHash,
            Role = source.Role,
            CreatedDate = source.CreatedDate
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (Id, Name, Email, PasswordHash, Role, CreatedDate)
        VALUES (source.Id, source.Name, source.Email, source.PasswordHash, source.Role, source.CreatedDate);
END;

SET IDENTITY_INSERT dbo.Users OFF;

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
    UPDATE SET
        Name = source.Name,
        Description = source.Description,
        ImageUrl = source.ImageUrl
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
    UPDATE SET
        ToolId = source.ToolId,
        ImageUrl = source.ImageUrl,
        DisplayOrder = source.DisplayOrder,
        UploadedDate = source.UploadedDate
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, ToolId, ImageUrl, DisplayOrder, UploadedDate)
    VALUES (source.Id, source.ToolId, source.ImageUrl, source.DisplayOrder, source.UploadedDate);

SET IDENTITY_INSERT dbo.ToolImages OFF;

SET IDENTITY_INSERT dbo.Reviews ON;

MERGE dbo.Reviews AS target
USING (VALUES
    (4001, 2001, 1, N'Test Customer', N'customer.test@reviewportal.local', N'The mixer arrived clean, started first time, and handled a full day of post and footing work without any issues.', 5, 4, 5, 4, 4, 4.40, N'Approved', NULL, CAST('2026-04-07T08:00:00' AS datetime2)),
    (4002, 2001, NULL, N'Glen Parker', N'glen.parker@example.test', N'Easy to load into the van and powerful enough for repeated batches. Pickup staff also explained the controls clearly.', 4, 4, 4, 5, 4, 4.20, N'Approved', NULL, CAST('2026-04-07T10:30:00' AS datetime2)),
    (4003, 2002, 1, N'Test Customer', N'customer.test@reviewportal.local', N'This cut through paving slabs and kerbs very quickly. The saw felt balanced and the blade tracked cleanly throughout.', 5, 5, 4, 5, 5, 4.80, N'Approved', NULL, CAST('2026-04-07T11:00:00' AS datetime2)),
    (4004, 2003, NULL, N'Sarah Foreman', N'sarah.foreman@example.test', N'Used it for cladding and site wash-down. Good pressure, simple setup, and no drop-off in performance during the hire.', 4, 4, 4, 5, 4, 4.20, N'Approved', NULL, CAST('2026-04-07T12:00:00' AS datetime2)),
    (4005, 2003, 1, N'Test Customer', N'customer.test@reviewportal.local', N'The washer was helpful overall, but I need more time to comment properly on fuel usage and hose length after the next hire.', 3, 4, 3, 4, 3, 3.40, N'Pending', NULL, CAST('2026-04-08T09:00:00' AS datetime2)),
    (4006, 2005, 1, N'Test Customer', N'customer.test@reviewportal.local', N'The rotavator handled compacted garden soil well and saved a lot of manual digging. Controls were straightforward to learn.', 4, 4, 4, 4, 4, 4.00, N'Approved', NULL, CAST('2026-04-07T13:30:00' AS datetime2)),
    (4007, 2005, NULL, N'Michelle Reed', N'michelle.reed@example.test', N'Plenty of torque for turning a neglected allotment plot. It was a little heavy to manoeuvre, but the end result was worth it.', 4, 3, 4, 4, 4, 3.80, N'Approved', NULL, CAST('2026-04-08T08:15:00' AS datetime2)),
    (4008, 2006, NULL, N'Alan Price', N'alan.price@example.test', N'The chipper completed the work, but the feedback was too vague to publish and the review did not meet moderation standards.', 2, 3, 2, 2, 2, 2.20, N'Rejected', N'Review rejected because the submission did not provide enough useful detail for publication.', CAST('2026-04-08T08:45:00' AS datetime2)),
    (4009, 2007, 1, N'Test Customer', N'customer.test@reviewportal.local', N'Very effective for drying a newly plastered room. Noticeably reduced moisture overnight and was simple to move between rooms.', 5, 4, 5, 4, 5, 4.60, N'Approved', NULL, CAST('2026-04-07T15:00:00' AS datetime2)),
    (4010, 2009, NULL, N'Ravi Singh', N'ravi.singh@example.test', N'Stable on level ground and ideal for decorating over stair landings. The platform gave a lot more confidence than standard ladders.', 4, 5, 4, 5, 4, 4.40, N'Approved', NULL, CAST('2026-04-07T16:00:00' AS datetime2)),
    (4011, 2011, 1, N'Test Customer', N'customer.test@reviewportal.local', N'The SDS drill coped well with anchor holes in dense concrete. Vibration was manageable and the supplied case kept everything tidy.', 4, 4, 4, 4, 4, 4.00, N'Approved', NULL, CAST('2026-04-08T07:45:00' AS datetime2)),
    (4012, 2012, NULL, N'Chris Nolan', N'chris.nolan@example.test', N'The breaker felt powerful during a short test run, but I have left this review pending until the full demolition job is complete.', 3, 3, 4, 3, 3, 3.20, N'Pending', NULL, CAST('2026-04-08T10:10:00' AS datetime2))
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

SET IDENTITY_INSERT dbo.ReviewComments ON;

MERGE dbo.ReviewComments AS target
USING (VALUES
    (5001, 4001, NULL, N'Paul Dawson', N'We hired the same mixer for a patio base and had a similarly solid experience with it.', N'Approved', CAST('2026-04-07T18:00:00' AS datetime2)),
    (5002, 4001, 1, N'Test Customer', N'The drum was also easy to rinse down at the end of the job, which saved time on return.', N'Approved', CAST('2026-04-07T18:30:00' AS datetime2)),
    (5003, 4004, 1, N'Test Customer', N'Good to know about the water supply setup. That matched our site experience too.', N'Approved', CAST('2026-04-07T19:00:00' AS datetime2)),
    (5004, 4005, NULL, N'Jordan Mills', N'I would like to see a follow-up once the longer hire is finished and the fuel use is clearer.', N'Pending', CAST('2026-04-08T11:00:00' AS datetime2)),
    (5005, 4008, NULL, N'Anonymous', N'This comment was rejected during moderation because it repeated the same complaint without adding detail.', N'Rejected', CAST('2026-04-08T11:30:00' AS datetime2)),
    (5006, 4010, NULL, N'Site Decorator Helen', N'The wider standing platform makes a noticeable difference when you are up there for most of the day.', N'Approved', CAST('2026-04-08T12:00:00' AS datetime2))
) AS source (Id, ReviewId, UserId, CommenterName, CommentText, Status, CreatedDate)
ON target.Id = source.Id
WHEN MATCHED THEN
    UPDATE SET
        ReviewId = source.ReviewId,
        UserId = source.UserId,
        CommenterName = source.CommenterName,
        CommentText = source.CommentText,
        Status = source.Status,
        CreatedDate = source.CreatedDate
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, ReviewId, UserId, CommenterName, CommentText, Status, CreatedDate)
    VALUES (source.Id, source.ReviewId, source.UserId, source.CommenterName, source.CommentText, source.Status, source.CreatedDate);

SET IDENTITY_INSERT dbo.ReviewComments OFF;

SET IDENTITY_INSERT dbo.CompanyResponses ON;

MERGE dbo.CompanyResponses AS target
USING (VALUES
    (6001, 4001, 2, N'Thanks for the detailed feedback. We have passed your comments on to the depot team and are glad the mixer performed well for the full job.', CAST('2026-04-07T20:00:00' AS datetime2), CAST('2026-04-07T20:00:00' AS datetime2)),
    (6002, 4004, 3, N'We appreciate the note about setup. We now include that same water-supply reminder in the hire handover for this washer.', CAST('2026-04-07T20:30:00' AS datetime2), CAST('2026-04-07T20:30:00' AS datetime2)),
    (6003, 4010, 2, N'Thank you for highlighting the stability of the platform ladder. We are pleased it helped your decorating work go more smoothly.', CAST('2026-04-08T13:00:00' AS datetime2), CAST('2026-04-08T13:00:00' AS datetime2))
) AS source (Id, ReviewId, StaffUserId, ResponseText, CreatedDate, UpdatedDate)
ON target.Id = source.Id
WHEN MATCHED THEN
    UPDATE SET
        ReviewId = source.ReviewId,
        StaffUserId = source.StaffUserId,
        ResponseText = source.ResponseText,
        CreatedDate = source.CreatedDate,
        UpdatedDate = source.UpdatedDate
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, ReviewId, StaffUserId, ResponseText, CreatedDate, UpdatedDate)
    VALUES (source.Id, source.ReviewId, source.StaffUserId, source.ResponseText, source.CreatedDate, source.UpdatedDate);

SET IDENTITY_INSERT dbo.CompanyResponses OFF;

COMMIT TRANSACTION;

PRINT 'ReviewPortal full relational test data seeded successfully.';
