BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423094500_SeedEpic2CommentAndResponseData'
)
BEGIN
    SET IDENTITY_INSERT dbo.ReviewComments ON;

    MERGE dbo.ReviewComments AS target
    USING (VALUES
        (5001, 4001, NULL, N'Mason Reed', N'We hired the same mixer for a footing pour and had the same smooth handover. Good to know the setup advice was consistent.', N'Approved', NULL, CAST('2026-04-10T12:10:00' AS datetime2)),
        (5002, 4003, 1, N'Test Customer', N'We used this saw on old slabs and the branch guidance on water suppression made a real difference on site.', N'Approved', NULL, CAST('2026-04-11T09:40:00' AS datetime2)),
        (5003, 4005, NULL, N'Rachel Kent', N'This matched our garden clearance job too. The machine was easier to control after the first pass and the fuel guidance helped.', N'Approved', NULL, CAST('2026-04-14T18:05:00' AS datetime2)),
        (5004, 4007, NULL, N'Pending Customer', N'Can a moderator check whether the extension lead advice should be added to the public guidance for this hire?', N'Pending', NULL, CAST('2026-04-16T10:20:00' AS datetime2))
    ) AS source (Id, ReviewId, UserId, CommenterName, CommentText, Status, RejectionReason, CreatedDate)
    ON target.Id = source.Id
    WHEN MATCHED THEN
        UPDATE SET
            ReviewId = source.ReviewId,
            UserId = source.UserId,
            CommenterName = source.CommenterName,
            CommentText = source.CommentText,
            Status = source.Status,
            RejectionReason = source.RejectionReason,
            CreatedDate = source.CreatedDate
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (Id, ReviewId, UserId, CommenterName, CommentText, Status, RejectionReason, CreatedDate)
        VALUES (source.Id, source.ReviewId, source.UserId, source.CommenterName, source.CommentText, source.Status, source.RejectionReason, source.CreatedDate);

    SET IDENTITY_INSERT dbo.ReviewComments OFF;

    SET IDENTITY_INSERT dbo.CompanyResponses ON;

    MERGE dbo.CompanyResponses AS target
    USING (VALUES
        (6001, 4001, 2, N'Thanks for the detailed feedback, Alice. We''re glad the handover and equipment prep made the weekend job easier.', CAST('2026-04-10T15:00:00' AS datetime2), CAST('2026-04-10T15:00:00' AS datetime2)),
        (6002, 4005, 2, N'Thanks, Emma. We appreciate the note on collection timing and we''re pleased the rotavator coped well with the compacted ground.', CAST('2026-04-15T08:45:00' AS datetime2), CAST('2026-04-15T08:45:00' AS datetime2))
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423094500_SeedEpic2CommentAndResponseData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260423094500_SeedEpic2CommentAndResponseData', N'8.0.11');
END;
GO

COMMIT;
GO

