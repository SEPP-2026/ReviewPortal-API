IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [ImageUrl] nvarchar(500) NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Email] nvarchar(256) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Role] nvarchar(50) NOT NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Tools] (
    [Id] int NOT NULL IDENTITY,
    [CategoryId] int NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(2000) NOT NULL,
    [HourlyRate] decimal(10,2) NOT NULL,
    [DailyRate] decimal(10,2) NOT NULL,
    [WeeklyRate] decimal(10,2) NOT NULL,
    [SpecialNotes] nvarchar(1000) NULL,
    [DepositRequired] bit NOT NULL,
    [DepositAmount] decimal(10,2) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [OverallRating] decimal(3,2) NULL,
    [ReviewCount] int NOT NULL DEFAULT 0,
    [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_Tools] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Tools_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Reviews] (
    [Id] int NOT NULL IDENTITY,
    [ToolId] int NOT NULL,
    [UserId] int NULL,
    [ReviewerName] nvarchar(100) NOT NULL,
    [ReviewerEmail] nvarchar(256) NOT NULL,
    [ReviewText] nvarchar(2000) NOT NULL,
    [EquipmentRating] int NOT NULL,
    [CustomerServiceRating] int NOT NULL,
    [TechnicalSupportRating] int NOT NULL,
    [AfterSalesRating] int NOT NULL,
    [ValueForMoneyRating] int NOT NULL,
    [OverallRating] decimal(3,2) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [RejectionReason] nvarchar(500) NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_Reviews] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Reviews_Tools_ToolId] FOREIGN KEY ([ToolId]) REFERENCES [Tools] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Reviews_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [ToolImages] (
    [Id] int NOT NULL IDENTITY,
    [ToolId] int NOT NULL,
    [ImageUrl] nvarchar(500) NOT NULL,
    [DisplayOrder] int NOT NULL DEFAULT 0,
    [UploadedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_ToolImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ToolImages_Tools_ToolId] FOREIGN KEY ([ToolId]) REFERENCES [Tools] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [CompanyResponses] (
    [Id] int NOT NULL IDENTITY,
    [ReviewId] int NOT NULL,
    [StaffUserId] int NOT NULL,
    [ResponseText] nvarchar(2000) NOT NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_CompanyResponses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CompanyResponses_Reviews_ReviewId] FOREIGN KEY ([ReviewId]) REFERENCES [Reviews] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CompanyResponses_Users_StaffUserId] FOREIGN KEY ([StaffUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ReviewComments] (
    [Id] int NOT NULL IDENTITY,
    [ReviewId] int NOT NULL,
    [UserId] int NULL,
    [CommenterName] nvarchar(100) NOT NULL,
    [CommentText] nvarchar(1000) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_ReviewComments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ReviewComments_Reviews_ReviewId] FOREIGN KEY ([ReviewId]) REFERENCES [Reviews] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ReviewComments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedDate', N'Email', N'Name', N'PasswordHash', N'Role') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([Id], [CreatedDate], [Email], [Name], [PasswordHash], [Role])
VALUES (1, '2026-04-06T00:00:00.0000000Z', N'customer.test@reviewportal.local', N'Test Customer', N'AQAAAAIAAYagAAAAEA0G/BQ4WPdNpKqAnK0Mam0BNoG560l4GFfcdOa4Xps8ZYyGiiH6yxH5EgC1llfMqQ==', N'Customer'),
(2, '2026-04-06T00:00:00.0000000Z', N'admin.test@reviewportal.local', N'Test Admin', N'AQAAAAIAAYagAAAAEOqaZaaWb29MnbnePdQNWq+wQO66MHMXfZh0ouwcBZjORmC3mllgI0zebkL9iOSFqQ==', N'Admin'),
(3, '2026-04-06T00:00:00.0000000Z', N'moderator.test@reviewportal.local', N'Test Moderator', N'AQAAAAIAAYagAAAAEMnrqmN8EAAHiVorrGMGoFopVvlvMBVCFw0pvQz9mgWzxZrB1RGNwUsqv+n41Wss/g==', N'Moderator');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedDate', N'Email', N'Name', N'PasswordHash', N'Role') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;
GO

CREATE UNIQUE INDEX [IX_Categories_Name] ON [Categories] ([Name]);
GO

CREATE UNIQUE INDEX [IX_CompanyResponses_ReviewId] ON [CompanyResponses] ([ReviewId]);
GO

CREATE INDEX [IX_CompanyResponses_StaffUserId] ON [CompanyResponses] ([StaffUserId]);
GO

CREATE INDEX [IX_ReviewComments_ReviewId] ON [ReviewComments] ([ReviewId]);
GO

CREATE INDEX [IX_ReviewComments_UserId] ON [ReviewComments] ([UserId]);
GO

CREATE INDEX [IX_Reviews_Status] ON [Reviews] ([Status]);
GO

CREATE INDEX [IX_Reviews_ToolId] ON [Reviews] ([ToolId]);
GO

CREATE INDEX [IX_Reviews_UserId] ON [Reviews] ([UserId]);
GO

CREATE INDEX [IX_ToolImages_ToolId] ON [ToolImages] ([ToolId]);
GO

CREATE INDEX [IX_Tools_CategoryId] ON [Tools] ([CategoryId]);
GO

CREATE INDEX [IX_Tools_IsActive] ON [Tools] ([IsActive]);
GO

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260406191534_InitialCreate', N'8.0.11');
GO

COMMIT;
GO

