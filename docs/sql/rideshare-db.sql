IF OBJECT_ID(N'[__EFMigrationsHistory_RideShare]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory_RideShare] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory_RideShare] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE TABLE [CopilotConversations] (
        [Id] uniqueidentifier NOT NULL,
        [ApplicationUserId] uniqueidentifier NOT NULL,
        [Title] nvarchar(120) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_CopilotConversations] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE TABLE [DriverProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [ApplicationUserId] uniqueidentifier NOT NULL,
        [FullName] nvarchar(120) NOT NULL,
        [PhoneNumber] nvarchar(20) NOT NULL,
        [LicenseNumber] nvarchar(40) NOT NULL,
        [AvailabilityStatus] nvarchar(20) NOT NULL,
        [AverageRating] float NOT NULL,
        [RatingCount] int NOT NULL,
        [StripeConnectedAccountId] nvarchar(100) NULL,
        [LastKnownLatitude] float NULL,
        [LastKnownLongitude] float NULL,
        [LastLocationUpdateUtc] datetime2 NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_DriverProfiles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] uniqueidentifier NOT NULL,
        [RideId] uniqueidentifier NOT NULL,
        [Amount] decimal(10,2) NOT NULL,
        [Currency] nvarchar(3) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [StripePaymentIntentId] nvarchar(100) NOT NULL,
        [FailureReason] nvarchar(300) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE TABLE [Ratings] (
        [Id] uniqueidentifier NOT NULL,
        [RideId] uniqueidentifier NOT NULL,
        [RaterProfileId] uniqueidentifier NOT NULL,
        [RateeProfileId] uniqueidentifier NOT NULL,
        [Direction] nvarchar(20) NOT NULL,
        [Stars] int NOT NULL,
        [Comment] nvarchar(500) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Ratings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE TABLE [RiderProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [ApplicationUserId] uniqueidentifier NOT NULL,
        [FullName] nvarchar(120) NOT NULL,
        [PhoneNumber] nvarchar(20) NOT NULL,
        [StripeCustomerId] nvarchar(100) NULL,
        [AverageRating] float NOT NULL,
        [RatingCount] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_RiderProfiles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE TABLE [Rides] (
        [Id] uniqueidentifier NOT NULL,
        [RiderProfileId] uniqueidentifier NOT NULL,
        [DriverProfileId] uniqueidentifier NULL,
        [VehicleId] uniqueidentifier NULL,
        [PickupLatitude] float NOT NULL,
        [PickupLongitude] float NOT NULL,
        [PickupAddress] nvarchar(300) NULL,
        [DropoffLatitude] float NOT NULL,
        [DropoffLongitude] float NOT NULL,
        [DropoffAddress] nvarchar(300) NULL,
        [RequestedVehicleType] nvarchar(20) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [EstimatedFareAmount] decimal(10,2) NOT NULL,
        [EstimatedFareCurrency] nvarchar(3) NOT NULL,
        [FinalFareAmount] decimal(10,2) NULL,
        [FinalFareCurrency] nvarchar(3) NULL,
        [EstimatedDistanceKm] float NOT NULL,
        [RequestedAtUtc] datetime2 NOT NULL,
        [DriverAssignedAtUtc] datetime2 NULL,
        [StartedAtUtc] datetime2 NULL,
        [CompletedAtUtc] datetime2 NULL,
        [CancelledAtUtc] datetime2 NULL,
        [CancellationReason] nvarchar(300) NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Rides] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE TABLE [Vehicles] (
        [Id] uniqueidentifier NOT NULL,
        [DriverProfileId] uniqueidentifier NOT NULL,
        [Make] nvarchar(60) NOT NULL,
        [Model] nvarchar(60) NOT NULL,
        [Year] int NOT NULL,
        [Color] nvarchar(30) NOT NULL,
        [PlateNumber] nvarchar(15) NOT NULL,
        [VehicleType] nvarchar(20) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Vehicles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE TABLE [CopilotMessages] (
        [Id] uniqueidentifier NOT NULL,
        [CopilotConversationId] uniqueidentifier NOT NULL,
        [Role] nvarchar(20) NOT NULL,
        [Content] nvarchar(4000) NOT NULL,
        [ToolCallJson] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_CopilotMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CopilotMessages_CopilotConversations_CopilotConversationId] FOREIGN KEY ([CopilotConversationId]) REFERENCES [CopilotConversations] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CopilotConversations_ApplicationUserId] ON [CopilotConversations] ([ApplicationUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CopilotMessages_CopilotConversationId] ON [CopilotMessages] ([CopilotConversationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DriverProfiles_ApplicationUserId] ON [DriverProfiles] ([ApplicationUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DriverProfiles_LastKnownLatitude_LastKnownLongitude] ON [DriverProfiles] ([LastKnownLatitude], [LastKnownLongitude]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Payments_RideId] ON [Payments] ([RideId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Payments_StripePaymentIntentId] ON [Payments] ([StripePaymentIntentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Ratings_RideId_RaterProfileId] ON [Ratings] ([RideId], [RaterProfileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RiderProfiles_ApplicationUserId] ON [RiderProfiles] ([ApplicationUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Rides_DriverProfileId] ON [Rides] ([DriverProfileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Rides_RiderProfileId] ON [Rides] ([RiderProfileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Rides_Status] ON [Rides] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Vehicles_DriverProfileId] ON [Vehicles] ([DriverProfileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922091432_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory_RideShare] ([MigrationId], [ProductVersion])
    VALUES (N'20260922091432_InitialCreate', N'9.0.20');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922092429_AddPaymentClientSecret'
)
BEGIN
    ALTER TABLE [Payments] ADD [ClientSecret] nvarchar(200) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922092429_AddPaymentClientSecret'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory_RideShare] ([MigrationId], [ProductVersion])
    VALUES (N'20260922092429_AddPaymentClientSecret', N'9.0.20');
END;

COMMIT;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922174437_AddKnowledgeArticlesTable'
)
BEGIN
    ALTER DATABASE SCOPED CONFIGURATION SET PREVIEW_FEATURES = ON;
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922174437_AddKnowledgeArticlesTable'
)
BEGIN
    CREATE TABLE KnowledgeArticles (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        Category NVARCHAR(60) NOT NULL,
        Embedding VECTOR(1536) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL
    );
END;

COMMIT;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_RideShare]
    WHERE [MigrationId] = N'20260922174437_AddKnowledgeArticlesTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory_RideShare] ([MigrationId], [ProductVersion])
    VALUES (N'20260922174437_AddKnowledgeArticlesTable', N'9.0.20');
END;
GO

