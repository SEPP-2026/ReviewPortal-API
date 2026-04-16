/*
  ReviewPortal password reset schema update

  Adds the password reset token columns required by the forgot/reset password API.
  Safe to run multiple times. Existing data is preserved.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF COL_LENGTH('dbo.Users', 'PasswordResetTokenHash') IS NULL
BEGIN
    ALTER TABLE dbo.Users
    ADD PasswordResetTokenHash nvarchar(256) NULL;
END;

IF COL_LENGTH('dbo.Users', 'PasswordResetTokenExpiryUtc') IS NULL
BEGIN
    ALTER TABLE dbo.Users
    ADD PasswordResetTokenExpiryUtc datetime2 NULL;
END;

COMMIT TRANSACTION;

PRINT 'ReviewPortal password reset schema update applied successfully.';
