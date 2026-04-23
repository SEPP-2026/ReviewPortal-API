BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422213000_SeedRemainingEpic1CatalogueData'
)
BEGIN
    SET IDENTITY_INSERT dbo.Categories ON;

    MERGE dbo.Categories AS target
    USING (VALUES
        (1007, N'Painting & Decorating', N'Decorating, surface preparation, and finishing equipment for domestic and commercial jobs.', N'https://cdn.reviewportal.local/categories/painting-decorating.jpg'),
        (1008, N'Plumbing & Drainage', N'Pipework, drain clearance, and plumbing maintenance equipment for site and home repairs.', N'https://cdn.reviewportal.local/categories/plumbing-drainage.jpg'),
        (1009, N'Services', N'Non-physical hire services including delivery, operator hire, and compliance testing', N'https://cdn.reviewportal.local/categories/services.jpg')
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
        (2019, 1007, N'Paint Sprayer HVLP', N'High-volume low-pressure sprayer for smooth emulsion, varnish, and stain finishes across walls, fences, and trim.', 11.00, 42.00, 140.00, N'Strain paint before use and clean the nozzle immediately after each hire period.', 1, 75.00, 1, NULL, 0, CAST('2026-04-22T08:00:00' AS datetime2), CAST('2026-04-22T08:00:00' AS datetime2)),
        (2020, 1007, N'Wallpaper Steamer', N'Professional wallpaper stripper with large steam plate for removing stubborn coverings from plaster and painted walls.', 8.00, 28.00, 92.00, N'Allow walls to dry fully before sanding or redecorating.', 0, NULL, 1, NULL, 0, CAST('2026-04-22T08:10:00' AS datetime2), CAST('2026-04-22T08:10:00' AS datetime2)),
        (2021, 1007, N'Belt Sander 240V', N'Heavy-duty belt sander for levelling timber boards, doors, and rough joinery before finishing.', 10.00, 36.00, 118.00, N'Sanding belts are supplied separately and charged according to grit and wear.', 1, 65.00, 1, NULL, 0, CAST('2026-04-22T08:20:00' AS datetime2), CAST('2026-04-22T08:20:00' AS datetime2)),
        (2022, 1007, N'Heat Gun Kit', N'Variable-temperature heat gun kit for paint stripping, adhesive softening, and controlled surface preparation.', 6.00, 22.00, 70.00, N'Use in well-ventilated areas and keep away from flammable materials.', 0, NULL, 1, NULL, 0, CAST('2026-04-22T08:30:00' AS datetime2), CAST('2026-04-22T08:30:00' AS datetime2)),
        (2023, 1008, N'Pipe Freezing Kit', N'Portable pipe freezing kit for isolating copper and plastic pipework without draining the full system.', 18.00, 68.00, 225.00, N'Freezing jackets and consumables must match the pipe diameter.', 1, 125.00, 1, NULL, 0, CAST('2026-04-22T08:40:00' AS datetime2), CAST('2026-04-22T08:40:00' AS datetime2)),
        (2024, 1008, N'Drain Rod Set', N'Flexible drain rod set with plunger and scraper attachments for clearing domestic and light commercial blockages.', 7.00, 24.00, 78.00, N'Wear protective gloves and disinfect rods after use.', 0, NULL, 1, NULL, 0, CAST('2026-04-22T08:50:00' AS datetime2), CAST('2026-04-22T08:50:00' AS datetime2)),
        (2025, 1008, N'Ratchet Pipe Cutter', N'Ratchet pipe cutter for clean, square cuts through plastic and multilayer plumbing pipe.', 5.00, 18.00, 58.00, N'Check blade condition before cutting pressure-rated pipework.', 0, NULL, 1, NULL, 0, CAST('2026-04-22T09:00:00' AS datetime2), CAST('2026-04-22T09:00:00' AS datetime2)),
        (2026, 1008, N'Plumber''s Torch Kit', N'Portable torch kit for soldering, heat work, and small plumbing repairs where controlled flame is required.', 9.00, 34.00, 112.00, N'Gas cartridges are supplied separately. Hot works permits may be required on site.', 1, 70.00, 1, NULL, 0, CAST('2026-04-22T09:10:00' AS datetime2), CAST('2026-04-22T09:10:00' AS datetime2)),
        (2027, 1009, N'Equipment Delivery', N'Booked delivery and collection service for hired equipment within the Shelton local service area.', 15.00, 55.00, 180.00, N'Delivery windows depend on route availability and equipment size.', 0, NULL, 1, NULL, 0, CAST('2026-04-22T09:20:00' AS datetime2), CAST('2026-04-22T09:20:00' AS datetime2)),
        (2028, 1009, N'Trained Operator Hire', N'Qualified operator support for specialist hire items where supervised or assisted operation is required.', 32.00, 240.00, 950.00, N'Operator availability must be confirmed before booking high-demand dates.', 0, NULL, 1, NULL, 0, CAST('2026-04-22T09:30:00' AS datetime2), CAST('2026-04-22T09:30:00' AS datetime2)),
        (2029, 1009, N'PAT Testing Service', N'Portable appliance testing service for hired or customer-owned site electrical equipment.', 25.00, 180.00, 700.00, N'Includes test labels and a digital summary certificate after completion.', 0, NULL, 1, NULL, 0, CAST('2026-04-22T09:40:00' AS datetime2), CAST('2026-04-22T09:40:00' AS datetime2)),
        (2030, 1009, N'Site Survey', N'Pre-hire site survey to confirm access, equipment suitability, safety requirements, and delivery constraints.', 45.00, 300.00, 1100.00, N'Recommended for access, lifting, and operator-assisted hires.', 0, NULL, 1, NULL, 0, CAST('2026-04-22T09:50:00' AS datetime2), CAST('2026-04-22T09:50:00' AS datetime2))
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
        (3037, 2019, N'https://cdn.reviewportal.local/tools/paint-sprayer-1.jpg', 1, CAST('2026-04-22T10:00:00' AS datetime2)),
        (3038, 2019, N'https://cdn.reviewportal.local/tools/paint-sprayer-2.jpg', 2, CAST('2026-04-22T10:01:00' AS datetime2)),
        (3039, 2020, N'https://cdn.reviewportal.local/tools/wallpaper-steamer-1.jpg', 1, CAST('2026-04-22T10:02:00' AS datetime2)),
        (3040, 2020, N'https://cdn.reviewportal.local/tools/wallpaper-steamer-2.jpg', 2, CAST('2026-04-22T10:03:00' AS datetime2)),
        (3041, 2021, N'https://cdn.reviewportal.local/tools/belt-sander-1.jpg', 1, CAST('2026-04-22T10:04:00' AS datetime2)),
        (3042, 2021, N'https://cdn.reviewportal.local/tools/belt-sander-2.jpg', 2, CAST('2026-04-22T10:05:00' AS datetime2)),
        (3043, 2022, N'https://cdn.reviewportal.local/tools/heat-gun-1.jpg', 1, CAST('2026-04-22T10:06:00' AS datetime2)),
        (3044, 2022, N'https://cdn.reviewportal.local/tools/heat-gun-2.jpg', 2, CAST('2026-04-22T10:07:00' AS datetime2)),
        (3045, 2023, N'https://cdn.reviewportal.local/tools/pipe-freezing-kit-1.jpg', 1, CAST('2026-04-22T10:08:00' AS datetime2)),
        (3046, 2023, N'https://cdn.reviewportal.local/tools/pipe-freezing-kit-2.jpg', 2, CAST('2026-04-22T10:09:00' AS datetime2)),
        (3047, 2024, N'https://cdn.reviewportal.local/tools/drain-rod-set-1.jpg', 1, CAST('2026-04-22T10:10:00' AS datetime2)),
        (3048, 2024, N'https://cdn.reviewportal.local/tools/drain-rod-set-2.jpg', 2, CAST('2026-04-22T10:11:00' AS datetime2)),
        (3049, 2025, N'https://cdn.reviewportal.local/tools/pipe-cutter-1.jpg', 1, CAST('2026-04-22T10:12:00' AS datetime2)),
        (3050, 2025, N'https://cdn.reviewportal.local/tools/pipe-cutter-2.jpg', 2, CAST('2026-04-22T10:13:00' AS datetime2)),
        (3051, 2026, N'https://cdn.reviewportal.local/tools/plumbers-torch-1.jpg', 1, CAST('2026-04-22T10:14:00' AS datetime2)),
        (3052, 2026, N'https://cdn.reviewportal.local/tools/plumbers-torch-2.jpg', 2, CAST('2026-04-22T10:15:00' AS datetime2)),
        (3053, 2027, N'https://cdn.reviewportal.local/tools/equipment-delivery-1.jpg', 1, CAST('2026-04-22T10:16:00' AS datetime2)),
        (3054, 2027, N'https://cdn.reviewportal.local/tools/equipment-delivery-2.jpg', 2, CAST('2026-04-22T10:17:00' AS datetime2)),
        (3055, 2028, N'https://cdn.reviewportal.local/tools/trained-operator-hire-1.jpg', 1, CAST('2026-04-22T10:18:00' AS datetime2)),
        (3056, 2028, N'https://cdn.reviewportal.local/tools/trained-operator-hire-2.jpg', 2, CAST('2026-04-22T10:19:00' AS datetime2)),
        (3057, 2029, N'https://cdn.reviewportal.local/tools/pat-testing-service-1.jpg', 1, CAST('2026-04-22T10:20:00' AS datetime2)),
        (3058, 2029, N'https://cdn.reviewportal.local/tools/pat-testing-service-2.jpg', 2, CAST('2026-04-22T10:21:00' AS datetime2)),
        (3059, 2030, N'https://cdn.reviewportal.local/tools/site-survey-1.jpg', 1, CAST('2026-04-22T10:22:00' AS datetime2)),
        (3060, 2030, N'https://cdn.reviewportal.local/tools/site-survey-2.jpg', 2, CAST('2026-04-22T10:23:00' AS datetime2))
    ) AS source (Id, ToolId, ImageUrl, DisplayOrder, UploadedDate)
    ON target.Id = source.Id
    WHEN MATCHED THEN
        UPDATE SET ToolId = source.ToolId, ImageUrl = source.ImageUrl, DisplayOrder = source.DisplayOrder, UploadedDate = source.UploadedDate
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (Id, ToolId, ImageUrl, DisplayOrder, UploadedDate)
        VALUES (source.Id, source.ToolId, source.ImageUrl, source.DisplayOrder, source.UploadedDate);

    SET IDENTITY_INSERT dbo.ToolImages OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422213000_SeedRemainingEpic1CatalogueData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260422213000_SeedRemainingEpic1CatalogueData', N'8.0.11');
END;
GO

COMMIT;
GO

