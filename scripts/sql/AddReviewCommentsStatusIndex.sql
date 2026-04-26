BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260426210328_AddReviewCommentsStatusIndex'
)
BEGIN
    CREATE INDEX [IX_ReviewComments_Status] ON [ReviewComments] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260426210328_AddReviewCommentsStatusIndex'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260426210328_AddReviewCommentsStatusIndex', N'8.0.11');
END;
GO

COMMIT;
GO

