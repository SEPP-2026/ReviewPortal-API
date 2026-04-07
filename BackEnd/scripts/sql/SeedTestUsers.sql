/*
  ReviewPortal test users
  Customer login: customer.test@reviewportal.local / Customer123!
  Admin login: admin.test@reviewportal.local / Admin123!
  Moderator login: moderator.test@reviewportal.local / Moderator123!
*/

SET NOCOUNT ON;

BEGIN TRANSACTION;

IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'customer.test@reviewportal.local')
BEGIN
    UPDATE dbo.Users
    SET
        Name = 'Test Customer',
        PasswordHash = 'AQAAAAIAAYagAAAAEA0G/BQ4WPdNpKqAnK0Mam0BNoG560l4GFfcdOa4Xps8ZYyGiiH6yxH5EgC1llfMqQ==',
        Role = 'Customer'
    WHERE Email = 'customer.test@reviewportal.local';
END
ELSE
BEGIN
    INSERT INTO dbo.Users (Name, Email, PasswordHash, Role, CreatedDate)
    VALUES ('Test Customer', 'customer.test@reviewportal.local', 'AQAAAAIAAYagAAAAEA0G/BQ4WPdNpKqAnK0Mam0BNoG560l4GFfcdOa4Xps8ZYyGiiH6yxH5EgC1llfMqQ==', 'Customer', SYSUTCDATETIME());
END;

IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'admin.test@reviewportal.local')
BEGIN
    UPDATE dbo.Users
    SET
        Name = 'Test Admin',
        PasswordHash = 'AQAAAAIAAYagAAAAEOqaZaaWb29MnbnePdQNWq+wQO66MHMXfZh0ouwcBZjORmC3mllgI0zebkL9iOSFqQ==',
        Role = 'Admin'
    WHERE Email = 'admin.test@reviewportal.local';
END
ELSE
BEGIN
    INSERT INTO dbo.Users (Name, Email, PasswordHash, Role, CreatedDate)
    VALUES ('Test Admin', 'admin.test@reviewportal.local', 'AQAAAAIAAYagAAAAEOqaZaaWb29MnbnePdQNWq+wQO66MHMXfZh0ouwcBZjORmC3mllgI0zebkL9iOSFqQ==', 'Admin', SYSUTCDATETIME());
END;

IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'moderator.test@reviewportal.local')
BEGIN
    UPDATE dbo.Users
    SET
        Name = 'Test Moderator',
        PasswordHash = 'AQAAAAIAAYagAAAAEMnrqmN8EAAHiVorrGMGoFopVvlvMBVCFw0pvQz9mgWzxZrB1RGNwUsqv+n41Wss/g==',
        Role = 'Moderator'
    WHERE Email = 'moderator.test@reviewportal.local';
END
ELSE
BEGIN
    INSERT INTO dbo.Users (Name, Email, PasswordHash, Role, CreatedDate)
    VALUES ('Test Moderator', 'moderator.test@reviewportal.local', 'AQAAAAIAAYagAAAAEMnrqmN8EAAHiVorrGMGoFopVvlvMBVCFw0pvQz9mgWzxZrB1RGNwUsqv+n41Wss/g==', 'Moderator', SYSUTCDATETIME());
END;

COMMIT TRANSACTION;
