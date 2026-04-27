BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260426213056_AddReviewRatingConstraints'
)
BEGIN
    EXEC(N'ALTER TABLE [Reviews] ADD CONSTRAINT [CK_Reviews_AfterSalesRating_Range] CHECK ([AfterSalesRating] BETWEEN 1 AND 5)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260426213056_AddReviewRatingConstraints'
)
BEGIN
    EXEC(N'ALTER TABLE [Reviews] ADD CONSTRAINT [CK_Reviews_CustomerServiceRating_Range] CHECK ([CustomerServiceRating] BETWEEN 1 AND 5)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260426213056_AddReviewRatingConstraints'
)
BEGIN
    EXEC(N'ALTER TABLE [Reviews] ADD CONSTRAINT [CK_Reviews_EquipmentRating_Range] CHECK ([EquipmentRating] BETWEEN 1 AND 5)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260426213056_AddReviewRatingConstraints'
)
BEGIN
    EXEC(N'ALTER TABLE [Reviews] ADD CONSTRAINT [CK_Reviews_TechnicalSupportRating_Range] CHECK ([TechnicalSupportRating] BETWEEN 1 AND 5)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260426213056_AddReviewRatingConstraints'
)
BEGIN
    EXEC(N'ALTER TABLE [Reviews] ADD CONSTRAINT [CK_Reviews_ValueForMoneyRating_Range] CHECK ([ValueForMoneyRating] BETWEEN 1 AND 5)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260426213056_AddReviewRatingConstraints'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260426213056_AddReviewRatingConstraints', N'8.0.11');
END;
GO

COMMIT;
GO

