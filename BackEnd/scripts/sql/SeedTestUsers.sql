/*
  ReviewPortal test users
  Customer login: customer.test@reviewportal.local / Customer123!
  Admin login: admin.test@reviewportal.local / Admin123!
  Moderator login: moderator.test@reviewportal.local / Moderator123!

  Safe to run multiple times.
  Re-running refreshes the seeded users back to their canonical test credentials
  and clears any stored password reset tokens when those columns exist.
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

IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'customer.test@reviewportal.local')
BEGIN
    IF @HasPasswordResetColumns = 1
    BEGIN
        UPDATE dbo.Users
        SET
            Name = 'Test Customer',
            PasswordHash = 'AQAAAAIAAYagAAAAEA0G/BQ4WPdNpKqAnK0Mam0BNoG560l4GFfcdOa4Xps8ZYyGiiH6yxH5EgC1llfMqQ==',
            PasswordResetTokenHash = NULL,
            PasswordResetTokenExpiryUtc = NULL,
            Role = 'Customer'
        WHERE Email = 'customer.test@reviewportal.local';
    END
    ELSE
    BEGIN
        UPDATE dbo.Users
        SET
            Name = 'Test Customer',
            PasswordHash = 'AQAAAAIAAYagAAAAEA0G/BQ4WPdNpKqAnK0Mam0BNoG560l4GFfcdOa4Xps8ZYyGiiH6yxH5EgC1llfMqQ==',
            Role = 'Customer'
        WHERE Email = 'customer.test@reviewportal.local';
    END
END
ELSE
BEGIN
    IF @HasPasswordResetColumns = 1
    BEGIN
        INSERT INTO dbo.Users (Name, Email, PasswordHash, PasswordResetTokenHash, PasswordResetTokenExpiryUtc, Role, CreatedDate)
        VALUES ('Test Customer', 'customer.test@reviewportal.local', 'AQAAAAIAAYagAAAAEA0G/BQ4WPdNpKqAnK0Mam0BNoG560l4GFfcdOa4Xps8ZYyGiiH6yxH5EgC1llfMqQ==', NULL, NULL, 'Customer', SYSUTCDATETIME());
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Users (Name, Email, PasswordHash, Role, CreatedDate)
        VALUES ('Test Customer', 'customer.test@reviewportal.local', 'AQAAAAIAAYagAAAAEA0G/BQ4WPdNpKqAnK0Mam0BNoG560l4GFfcdOa4Xps8ZYyGiiH6yxH5EgC1llfMqQ==', 'Customer', SYSUTCDATETIME());
    END
END;

IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'admin.test@reviewportal.local')
BEGIN
    IF @HasPasswordResetColumns = 1
    BEGIN
        UPDATE dbo.Users
        SET
            Name = 'Test Admin',
            PasswordHash = 'AQAAAAIAAYagAAAAEOqaZaaWb29MnbnePdQNWq+wQO66MHMXfZh0ouwcBZjORmC3mllgI0zebkL9iOSFqQ==',
            PasswordResetTokenHash = NULL,
            PasswordResetTokenExpiryUtc = NULL,
            Role = 'Admin'
        WHERE Email = 'admin.test@reviewportal.local';
    END
    ELSE
    BEGIN
        UPDATE dbo.Users
        SET
            Name = 'Test Admin',
            PasswordHash = 'AQAAAAIAAYagAAAAEOqaZaaWb29MnbnePdQNWq+wQO66MHMXfZh0ouwcBZjORmC3mllgI0zebkL9iOSFqQ==',
            Role = 'Admin'
        WHERE Email = 'admin.test@reviewportal.local';
    END
END
ELSE
BEGIN
    IF @HasPasswordResetColumns = 1
    BEGIN
        INSERT INTO dbo.Users (Name, Email, PasswordHash, PasswordResetTokenHash, PasswordResetTokenExpiryUtc, Role, CreatedDate)
        VALUES ('Test Admin', 'admin.test@reviewportal.local', 'AQAAAAIAAYagAAAAEOqaZaaWb29MnbnePdQNWq+wQO66MHMXfZh0ouwcBZjORmC3mllgI0zebkL9iOSFqQ==', NULL, NULL, 'Admin', SYSUTCDATETIME());
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Users (Name, Email, PasswordHash, Role, CreatedDate)
        VALUES ('Test Admin', 'admin.test@reviewportal.local', 'AQAAAAIAAYagAAAAEOqaZaaWb29MnbnePdQNWq+wQO66MHMXfZh0ouwcBZjORmC3mllgI0zebkL9iOSFqQ==', 'Admin', SYSUTCDATETIME());
    END
END;

IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'moderator.test@reviewportal.local')
BEGIN
    IF @HasPasswordResetColumns = 1
    BEGIN
        UPDATE dbo.Users
        SET
            Name = 'Test Moderator',
            PasswordHash = 'AQAAAAIAAYagAAAAEMnrqmN8EAAHiVorrGMGoFopVvlvMBVCFw0pvQz9mgWzxZrB1RGNwUsqv+n41Wss/g==',
            PasswordResetTokenHash = NULL,
            PasswordResetTokenExpiryUtc = NULL,
            Role = 'Moderator'
        WHERE Email = 'moderator.test@reviewportal.local';
    END
    ELSE
    BEGIN
        UPDATE dbo.Users
        SET
            Name = 'Test Moderator',
            PasswordHash = 'AQAAAAIAAYagAAAAEMnrqmN8EAAHiVorrGMGoFopVvlvMBVCFw0pvQz9mgWzxZrB1RGNwUsqv+n41Wss/g==',
            Role = 'Moderator'
        WHERE Email = 'moderator.test@reviewportal.local';
    END
END
ELSE
BEGIN
    IF @HasPasswordResetColumns = 1
    BEGIN
        INSERT INTO dbo.Users (Name, Email, PasswordHash, PasswordResetTokenHash, PasswordResetTokenExpiryUtc, Role, CreatedDate)
        VALUES ('Test Moderator', 'moderator.test@reviewportal.local', 'AQAAAAIAAYagAAAAEMnrqmN8EAAHiVorrGMGoFopVvlvMBVCFw0pvQz9mgWzxZrB1RGNwUsqv+n41Wss/g==', NULL, NULL, 'Moderator', SYSUTCDATETIME());
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Users (Name, Email, PasswordHash, Role, CreatedDate)
        VALUES ('Test Moderator', 'moderator.test@reviewportal.local', 'AQAAAAIAAYagAAAAEMnrqmN8EAAHiVorrGMGoFopVvlvMBVCFw0pvQz9mgWzxZrB1RGNwUsqv+n41Wss/g==', 'Moderator', SYSUTCDATETIME());
    END
END;

COMMIT TRANSACTION;

PRINT 'ReviewPortal test users seeded successfully.';
