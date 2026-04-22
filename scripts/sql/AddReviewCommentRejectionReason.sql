BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422221054_AddReviewCommentRejectionReason'
)
BEGIN
    ALTER TABLE [ReviewComments] ADD [RejectionReason] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422221054_AddReviewCommentRejectionReason'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260422221054_AddReviewCommentRejectionReason', N'8.0.11');
END;
GO

COMMIT;
GO

