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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] int NOT NULL IDENTITY,
        [Description] nvarchar(100) NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] int NOT NULL IDENTITY,
        [FullName] nvarchar(100) NOT NULL,
        [Address] nvarchar(250) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE TABLE [Banners] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(150) NOT NULL,
        [ImageUrl] nvarchar(500) NOT NULL,
        [LinkUrl] nvarchar(500) NULL,
        [SortOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Banners] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE TABLE [Brands] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Slug] nvarchar(120) NOT NULL,
        [LogoUrl] nvarchar(250) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Brands] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Slug] nvarchar(120) NOT NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE TABLE [News] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Slug] nvarchar(220) NOT NULL,
        [Summary] nvarchar(500) NULL,
        [Content] nvarchar(max) NULL,
        [ImageUrl] nvarchar(250) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_News] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] int NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] int NOT NULL,
        [RoleId] int NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] int NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE TABLE [ChatConversations] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NULL,
        [GuestToken] nvarchar(64) NULL,
        [GuestName] nvarchar(100) NULL,
        [IsClosed] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastMessageAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ChatConversations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChatConversations_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE TABLE [Orders] (
        [Id] int NOT NULL IDENTITY,
        [OrderCode] nvarchar(20) NOT NULL,
        [CustomerName] nvarchar(100) NOT NULL,
        [Phone] nvarchar(20) NOT NULL,
        [Email] nvarchar(150) NULL,
        [Address] nvarchar(250) NOT NULL,
        [Note] nvarchar(500) NULL,
        [Total] decimal(18,0) NOT NULL,
        [Status] int NOT NULL,
        [PaymentMethod] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UserId] int NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Orders_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE TABLE [Products] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Slug] nvarchar(220) NOT NULL,
        [Price] decimal(18,0) NOT NULL,
        [OldPrice] decimal(18,0) NULL,
        [Stock] int NOT NULL,
        [SoldCount] int NOT NULL,
        [ImageUrl] nvarchar(250) NULL,
        [Description] nvarchar(max) NULL,
        [IsFeatured] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CategoryId] int NOT NULL,
        [BrandId] int NOT NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Products_Brands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [Brands] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE TABLE [ChatMessages] (
        [Id] int NOT NULL IDENTITY,
        [ConversationId] int NOT NULL,
        [Sender] int NOT NULL,
        [Content] nvarchar(1000) NOT NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChatMessages_ChatConversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [ChatConversations] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE TABLE [OrderDetails] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ProductId] int NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,0) NOT NULL,
        CONSTRAINT [PK_OrderDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderDetails_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderDetails_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Brands_Slug] ON [Brands] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Categories_Slug] ON [Categories] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChatConversations_GuestToken] ON [ChatConversations] ([GuestToken]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChatConversations_LastMessageAt] ON [ChatConversations] ([LastMessageAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChatConversations_UserId] ON [ChatConversations] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChatMessages_ConversationId_CreatedAt] ON [ChatMessages] ([ConversationId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_News_Slug] ON [News] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderDetails_OrderId] ON [OrderDetails] ([OrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderDetails_ProductId] ON [OrderDetails] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Orders_OrderCode] ON [Orders] ([OrderCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_UserId] ON [Orders] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Products_BrandId] ON [Products] ([BrandId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Products_Slug] ON [Products] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150439_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028150439_InitialCreate', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028151056_ThemSoDiaChiVaDanhGia'
)
BEGIN
    CREATE TABLE [Addresses] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [ReceiverName] nvarchar(100) NOT NULL,
        [Phone] nvarchar(20) NOT NULL,
        [Detail] nvarchar(250) NOT NULL,
        [IsDefault] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Addresses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Addresses_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028151056_ThemSoDiaChiVaDanhGia'
)
BEGIN
    CREATE TABLE [Reviews] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [UserId] int NOT NULL,
        [Rating] int NOT NULL,
        [Comment] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Reviews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Reviews_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Reviews_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028151056_ThemSoDiaChiVaDanhGia'
)
BEGIN
    CREATE INDEX [IX_Addresses_UserId] ON [Addresses] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028151056_ThemSoDiaChiVaDanhGia'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Reviews_ProductId_UserId] ON [Reviews] ([ProductId], [UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028151056_ThemSoDiaChiVaDanhGia'
)
BEGIN
    CREATE INDEX [IX_Reviews_UserId] ON [Reviews] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028151056_ThemSoDiaChiVaDanhGia'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028151056_ThemSoDiaChiVaDanhGia', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030093817_ThemBangCoupon'
)
BEGIN
    CREATE TABLE [Coupons] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(30) NOT NULL,
        [Description] nvarchar(200) NULL,
        [DiscountType] int NOT NULL,
        [DiscountValue] decimal(18,0) NOT NULL,
        [MaxDiscount] decimal(18,0) NULL,
        [MinOrderValue] decimal(18,0) NOT NULL,
        [UsageLimit] int NULL,
        [UsedCount] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Coupons] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030093817_ThemBangCoupon'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Coupons_Code] ON [Coupons] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030093817_ThemBangCoupon'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261030093817_ThemBangCoupon', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030134729_ThemThongSoSanPham'
)
BEGIN
    ALTER TABLE [Products] ADD [Color] nvarchar(40) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030134729_ThemThongSoSanPham'
)
BEGIN
    ALTER TABLE [Products] ADD [GalleryUrls] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030134729_ThemThongSoSanPham'
)
BEGIN
    ALTER TABLE [Products] ADD [Os] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030134729_ThemThongSoSanPham'
)
BEGIN
    ALTER TABLE [Products] ADD [Ram] nvarchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030134729_ThemThongSoSanPham'
)
BEGIN
    ALTER TABLE [Products] ADD [Storage] nvarchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030134729_ThemThongSoSanPham'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261030134729_ThemThongSoSanPham', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101040124_ThemBangBienThe'
)
BEGIN
    CREATE TABLE [ProductVariants] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [Storage] nvarchar(30) NOT NULL,
        [Color] nvarchar(40) NOT NULL,
        [Price] decimal(18,0) NOT NULL,
        [OldPrice] decimal(18,0) NULL,
        [Stock] int NOT NULL,
        [ImageUrl] nvarchar(250) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_ProductVariants] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductVariants_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101040124_ThemBangBienThe'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductVariants_ProductId_Storage_Color] ON [ProductVariants] ([ProductId], [Storage], [Color]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101040124_ThemBangBienThe'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261101040124_ThemBangBienThe', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101090042_ThemThongSoKyThuat'
)
BEGIN
    ALTER TABLE [Products] ADD [Battery] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101090042_ThemThongSoKyThuat'
)
BEGIN
    ALTER TABLE [Products] ADD [Camera] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101090042_ThemThongSoKyThuat'
)
BEGIN
    ALTER TABLE [Products] ADD [Chip] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101090042_ThemThongSoKyThuat'
)
BEGIN
    ALTER TABLE [Products] ADD [Screen] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101090042_ThemThongSoKyThuat'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261101090042_ThemThongSoKyThuat', N'8.0.10');
END;
GO

COMMIT;
GO

