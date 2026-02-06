USE [master]
GO
/****** Object:  Database [WorkBotAI]    Script Date: 06/02/2026 15:24:36 ******/
CREATE DATABASE [WorkBotAI]
 CONTAINMENT = NONE
 ON  PRIMARY
( NAME = N'WorkBotAI', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\WorkBotAI.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON
( NAME = N'WorkBotAI_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\WorkBotAI_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [WorkBotAI] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [WorkBotAI].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [WorkBotAI] SET ANSI_NULL_DEFAULT OFF
GO
ALTER DATABASE [WorkBotAI] SET ANSI_NULLS OFF
GO
ALTER DATABASE [WorkBotAI] SET ANSI_PADDING OFF
GO
ALTER DATABASE [WorkBotAI] SET ANSI_WARNINGS OFF
GO
ALTER DATABASE [WorkBotAI] SET ARITHABORT OFF
GO
ALTER DATABASE [WorkBotAI] SET AUTO_CLOSE OFF
GO
ALTER DATABASE [WorkBotAI] SET AUTO_SHRINK OFF
GO
ALTER DATABASE [WorkBotAI] SET AUTO_UPDATE_STATISTICS ON
GO
ALTER DATABASE [WorkBotAI] SET CURSOR_CLOSE_ON_COMMIT OFF
GO
ALTER DATABASE [WorkBotAI] SET CURSOR_DEFAULT  GLOBAL
GO
ALTER DATABASE [WorkBotAI] SET CONCAT_NULL_YIELDS_NULL OFF
GO
ALTER DATABASE [WorkBotAI] SET NUMERIC_ROUNDABORT OFF
GO
ALTER DATABASE [WorkBotAI] SET QUOTED_IDENTIFIER OFF
GO
ALTER DATABASE [WorkBotAI] SET RECURSIVE_TRIGGERS OFF
GO
ALTER DATABASE [WorkBotAI] SET  DISABLE_BROKER
GO
ALTER DATABASE [WorkBotAI] SET AUTO_UPDATE_STATISTICS_ASYNC OFF
GO
ALTER DATABASE [WorkBotAI] SET DATE_CORRELATION_OPTIMIZATION OFF
GO
ALTER DATABASE [WorkBotAI] SET TRUSTWORTHY OFF
GO
ALTER DATABASE [WorkBotAI] SET ALLOW_SNAPSHOT_ISOLATION OFF
GO
ALTER DATABASE [WorkBotAI] SET PARAMETERIZATION SIMPLE
GO
ALTER DATABASE [WorkBotAI] SET READ_COMMITTED_SNAPSHOT OFF
GO
ALTER DATABASE [WorkBotAI] SET HONOR_BROKER_PRIORITY OFF
GO
ALTER DATABASE [WorkBotAI] SET RECOVERY SIMPLE
GO
ALTER DATABASE [WorkBotAI] SET  MULTI_USER
GO
ALTER DATABASE [WorkBotAI] SET PAGE_VERIFY CHECKSUM
GO
ALTER DATABASE [WorkBotAI] SET DB_CHAINING OFF
GO
ALTER DATABASE [WorkBotAI] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF )
GO
ALTER DATABASE [WorkBotAI] SET TARGET_RECOVERY_TIME = 60 SECONDS
GO
ALTER DATABASE [WorkBotAI] SET DELAYED_DURABILITY = DISABLED
GO
ALTER DATABASE [WorkBotAI] SET ACCELERATED_DATABASE_RECOVERY = OFF
GO
ALTER DATABASE [WorkBotAI] SET QUERY_STORE = ON
GO
ALTER DATABASE [WorkBotAI] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [WorkBotAI]
GO

----------------------------------------------------------------------------------------------------
-- TABLES
----------------------------------------------------------------------------------------------------

CREATE TABLE [dbo].[AppointmentStatus](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[IsActive] [bit] NULL DEFAULT ((1)),
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Categories](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[IsActive] [bit] NULL DEFAULT ((1)),
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[ContactInfoTypes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[PaymentStatus](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[IsActive] [bit] NULL DEFAULT ((1)),
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[PaymentTypes](
	[ID] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[IsActive] [bit] NULL DEFAULT ((1)),
	[BaseGateway] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Planes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[IsActive] [bit] NULL DEFAULT ((1)),
	[Description] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[ResourceTypes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[IsActive] [bit] NOT NULL DEFAULT ((1)),
	[CategoryID] [int] NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[SettingTypes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[SubscriptionStatus](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[IsActive] [bit] NULL DEFAULT ((1)),
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[UserStatuses](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[IsActive] [bit] NOT NULL DEFAULT ((1)),
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[JobTypes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[IsActive] [bit] NULL DEFAULT ((1)),
	[CategoryID] [int] NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Roles](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[IsActive] [bit] NOT NULL DEFAULT ((1)),
	[IsDeleted] [bit] NOT NULL DEFAULT ((0)),
	[DeletionDate] [datetime2](7) NULL,
	[DeletionUserID] [int] NULL,
	[CreationTime] [datetime2](7) NOT NULL DEFAULT (sysutcdatetime()),
	[CreationUserID] [int] NULL,
	[LastModificationTime] [datetime2](7) NULL,
	[LastModificationUserID] [int] NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Tenants](
	[ID] [uniqueidentifier] NOT NULL DEFAULT (newid()),
	[Name] [nvarchar](255) NOT NULL,
	[Acronym] [nvarchar](50) NULL,
	[LogoImage] [nvarchar](255) NULL,
	[CreationDate] [datetime2](7) NULL DEFAULT (sysutcdatetime()),
	[IsActive] [bit] NULL DEFAULT ((1)),
	[IsDeleted] [bit] NULL DEFAULT ((0)),
	[DeletionTime] [datetime2](7) NULL,
	[DeletionUserID] [int] NULL,
	[CategoryID] [int] NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Users](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
	[Password] [nvarchar](255) NOT NULL,
	[Mail] [nvarchar](256) NULL,
	[StatusID] [int] NOT NULL,
	[StatusTime] [datetime2](7) NULL,
	[LastLoginTime] [datetime2](7) NULL,
	[FirstName] [nvarchar](100) NULL,
	[LastName] [nvarchar](100) NULL,
	[IsActive] [bit] NULL DEFAULT ((1)),
	[CreationTime] [datetime2](7) NOT NULL DEFAULT (sysutcdatetime()),
	[CreationUserID] [int] NULL,
	[LastModificationTime] [datetime2](7) NULL,
	[LastModificationUserID] [int] NULL,
	[AvatarImage] [nvarchar](max) NULL,
	[RoleID] [int] NULL,
	[IsDeleted] [bit] NOT NULL DEFAULT ((0)),
	[DeletionTime] [datetime2](7) NULL,
	[DeletionUserID] [int] NULL,
	[TenantID] [uniqueidentifier] NULL,
	[IsSuperAdmin] [bit] NOT NULL DEFAULT ((0)),
	[VerificationToken] [nvarchar](max) NULL,
	[ResetPasswordCode] [nvarchar](2500) NULL,
	[FirstLoginOtp] [nvarchar](64) NULL,
	[FirstLoginExpire] [datetime2](7) NULL,
	[FirstLoginToken] [nvarchar](max) NULL,
	[ResetPasswordExpiration] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[Appointments](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TenantID] [uniqueidentifier] NOT NULL,
	[CustomerID] [int] NOT NULL,
	[StatusID] [int] NOT NULL,
	[StartTime] [datetime2](7) NOT NULL,
	[EndTime] [datetime2](7) NULL,
	[Note] [nvarchar](500) NULL,
	[IsActive] [bit] NULL DEFAULT ((1)),
	[StaffID] [int] NULL,
	[ResourceID] [int] NULL,
	[CreationTime] [datetime2](7) NOT NULL DEFAULT (sysutcdatetime()),
	[CreationUserID] [int] NULL,
	[LastModificationTime] [datetime2](7) NULL,
	[LastModificationUserID] [int] NULL,
	[IsDeleted] [bit] NOT NULL DEFAULT ((0)),
	[DeletionTime] [datetime2](7) NULL,
	[DeletionUserID] [int] NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[AppointmentPayments](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AppointmentID] [int] NOT NULL,
	[DatePayment] [datetime2](7) NOT NULL,
	[PaymentTypeID] [int] NOT NULL,
	[ImportValue] [decimal](18, 2) NOT NULL,
	[IvaValue] [decimal](18, 2) NOT NULL,
	[StatusPaymentID] [int] NOT NULL,
	[Note] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[AppointmentServices](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AppointmentID] [int] NOT NULL,
	[ServiceID] [int] NOT NULL,
	[Note] [nvarchar](300) NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Availabilities](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TenantID] [uniqueidentifier] NOT NULL,
	[StaffID] [int] NULL,
	[StartTime] [datetime2](7) NOT NULL,
	[EndTime] [datetime2](7) NOT NULL,
	[Note] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[ContactInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TenantID] [uniqueidentifier] NOT NULL,
	[TypeID] [int] NOT NULL,
	[Value] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Customers](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TenantID] [uniqueidentifier] NOT NULL,
	[FullName] [nvarchar](200) NOT NULL,
	[Phone] [nvarchar](50) NULL,
	[Email] [nvarchar](255) NULL,
	[Note] [nvarchar](500) NULL,
	[CreationTime] [datetime2](7) NOT NULL DEFAULT (sysutcdatetime()),
	[CreationUserID] [int] NULL,
	[LastModificationTime] [datetime2](7) NULL,
	[LastModificationUserID] [int] NULL,
	[IsDeleted] [bit] NOT NULL DEFAULT ((0)),
	[DeletionTime] [datetime2](7) NULL,
	[DeletionUserID] [int] NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Faqs](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CategoryID] [int] NULL,
	[Question] [nvarchar](500) NOT NULL,
	[IsActive] [bit] NOT NULL DEFAULT ((1)),
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Modules](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[IsActive] [bit] NOT NULL DEFAULT ((1)),
	[PlaneID] [int] NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Subscriptions](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TenantID] [uniqueidentifier] NOT NULL,
	[StatusID] [int] NOT NULL,
	[PlaneID] [int] NOT NULL,
	[StartDate] [date] NULL,
	[EndDate] [date] NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Payments](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SubscriptionID] [int] NOT NULL,
	[DatePayment] [datetime] NULL DEFAULT (getdate()),
	[PaymentTypeID] [int] NOT NULL,
	[ImportValue] [decimal](18, 2) NULL,
	[IvaValue] [decimal](18, 2) NULL,
	[StatusPaymentID] [int] NOT NULL,
	[Notes] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Submodules](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ModuleID] [int] NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[IsActive] [bit] NULL DEFAULT ((1)),
	[PlaneID] [int] NULL,
	[CreationTime] [datetime2](7) NOT NULL DEFAULT (sysutcdatetime()),
	[CreationUserID] [int] NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Permissions](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SubmoduleID] [int] NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[IsActive] [bit] NOT NULL DEFAULT ((1)),
	[IsMinimum] [bit] NOT NULL DEFAULT ((0)),
	[PlaneID] [int] NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Resources](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Code] [nvarchar](50) NULL,
	[NumeroPosti] [int] NULL DEFAULT ((1)),
	[IsAvailable] [bit] NOT NULL DEFAULT ((1)),
	[TypeID] [int] NOT NULL,
	[TenantID] [uniqueidentifier] NOT NULL,
	[CreationTime] [datetime2](7) NOT NULL DEFAULT (sysutcdatetime()),
	[CreationUserID] [int] NULL,
	[LastModificationTime] [datetime2](7) NULL,
	[LastModificationUserID] [int] NULL,
	[IsDeleted] [bit] NOT NULL DEFAULT ((0)),
	[DeletionTime] [datetime2](7) NULL,
	[DeletionUserID] [int] NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[RolePermissions](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RoleID] [int] NOT NULL,
	[PermissionID] [int] NOT NULL,
	[CreationTime] [datetime2](7) NOT NULL DEFAULT (sysutcdatetime()),
	[CreationUserID] [int] NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Services](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TenantID] [uniqueidentifier] NOT NULL,
	[CategoryID] [int] NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[DurationMin] [int] NULL,
	[BasePrice] [decimal](18, 2) NULL,
	[BafferTime] [decimal](18, 2) NULL,
	[IsActive] [bit] NULL DEFAULT ((1)),
	[CreationTime] [datetime2](7) NOT NULL DEFAULT (sysutcdatetime()),
	[CreationUserID] [int] NULL,
	[LastModificationTime] [datetime2](7) NULL,
	[LastModificationUserID] [int] NULL,
	[IsDeleted] [bit] NOT NULL DEFAULT ((0)),
	[DeletionTime] [datetime2](7) NULL,
	[DeletionUserID] [int] NULL,
	[ServiceTypeID] [int] NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[ServiceTypes](
	[ID] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[CategoryID] [int] NULL,
	[IsActive] [bit] NOT NULL DEFAULT ((0))
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Settings](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TenantID] [uniqueidentifier] NOT NULL,
	[SettingTypeID] [int] NOT NULL,
	[Value] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[Staff](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[TenantID] [uniqueidentifier] NOT NULL,
	[JobTypeID] [int] NULL,
	[IsActive] [bit] NULL DEFAULT ((1)),
	[CreationTime] [datetime2](7) NOT NULL DEFAULT (sysutcdatetime()),
	[CreationUserID] [int] NULL,
	[LastModificationTime] [datetime2](7) NULL,
	[LastModificationUserID] [int] NULL,
	[DeletionTime] [datetime2](7) NULL,
	[DeletionUserID] [int] NULL,
	[IsDeleted] [bit] NOT NULL DEFAULT ((0)),
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[TenantFaqs](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TenantID] [uniqueidentifier] NOT NULL,
	[FaqID] [int] NOT NULL,
	[Value] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL DEFAULT ((1)),
	[CreationTime] [datetime2](7) NOT NULL DEFAULT (sysutcdatetime()),
	[CreationUserID] [int] NULL,
	[LastModificationTime] [datetime2](7) NULL,
	[LastModificationUserID] [int] NULL,
	[IsDeleted] [bit] NOT NULL DEFAULT ((0)),
	[DeletionTime] [datetime2](7) NULL,
	[DeletionUserID] [int] NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

----------------------------------------------------------------------------------------------------
-- ADDITIONAL TABLES (Added during development)
----------------------------------------------------------------------------------------------------

CREATE TABLE [dbo].[SystemLogs](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Timestamp] [datetime2](7) NOT NULL DEFAULT (sysutcdatetime()),
	[Level] [nvarchar](50) NOT NULL,
	[Source] [nvarchar](255) NOT NULL,
	[Message] [nvarchar](max) NOT NULL,
	[Context] [nvarchar](max) NULL,
	[UserId] [int] NULL,
	[TenantId] [uniqueidentifier] NULL,
	[IpAddress] [nvarchar](50) NULL,
	[UserAgent] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [dbo].[SystemSettings](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Category] [nvarchar](100) NOT NULL,
	[Key] [nvarchar](100) NOT NULL,
	[Value] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[UpdatedAt] [datetime2](7) NOT NULL DEFAULT (sysutcdatetime()),
PRIMARY KEY CLUSTERED ([ID] ASC)) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------------------
-- INDEXES
----------------------------------------------------------------------------------------------------

CREATE NONCLUSTERED INDEX [IX_AppointmentPayments_Appointment] ON [dbo].[AppointmentPayments] ([AppointmentID] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Appointments_Customer] ON [dbo].[Appointments] ([CustomerID] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Appointments_Resource_Time] ON [dbo].[Appointments] ([ResourceID] ASC, [StartTime] ASC, [EndTime] ASC) WHERE ([IsDeleted]=(0))
GO
CREATE NONCLUSTERED INDEX [IX_Appointments_Staff_Time] ON [dbo].[Appointments] ([StaffID] ASC, [StartTime] ASC, [EndTime] ASC) WHERE ([IsDeleted]=(0))
GO
CREATE NONCLUSTERED INDEX [IX_Appointments_Tenant] ON [dbo].[Appointments] ([TenantID] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Availabilities_Staff] ON [dbo].[Availabilities] ([StaffID] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Availabilities_Tenant] ON [dbo].[Availabilities] ([TenantID] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Customers_Tenant] ON [dbo].[Customers] ([TenantID] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Faqs_Category] ON [dbo].[Faqs] ([CategoryID] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Resources_Type] ON [dbo].[Resources] ([TypeID] ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_Resources_Tenant_Code] ON [dbo].[Resources] ([TenantID] ASC, [Code] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_RolePermissions_Role] ON [dbo].[RolePermissions] ([RoleID] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Staff_JobType] ON [dbo].[Staff] ([JobTypeID] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Staff_Tenant] ON [dbo].[Staff] ([TenantID] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Users_IsActive] ON [dbo].[Users] ([IsActive] ASC) INCLUDE([TenantID])
GO
CREATE NONCLUSTERED INDEX [IX_Users_LastLoginTime] ON [dbo].[Users] ([LastLoginTime] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Users_Status] ON [dbo].[Users] ([StatusID] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Users_Tenant] ON [dbo].[Users] ([TenantID] ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_Users_Mail] ON [dbo].[Users] ([Mail] ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_Users_UserName] ON [dbo].[Users] ([UserName] ASC)
GO

----------------------------------------------------------------------------------------------------
-- FOREIGN KEYS
----------------------------------------------------------------------------------------------------

ALTER TABLE [dbo].[AppointmentPayments]  WITH CHECK ADD FOREIGN KEY([AppointmentID]) REFERENCES [dbo].[Appointments] ([ID])
GO
ALTER TABLE [dbo].[AppointmentPayments]  WITH CHECK ADD FOREIGN KEY([PaymentTypeID]) REFERENCES [dbo].[PaymentTypes] ([ID])
GO
ALTER TABLE [dbo].[AppointmentPayments]  WITH CHECK ADD FOREIGN KEY([StatusPaymentID]) REFERENCES [dbo].[PaymentStatus] ([ID])
GO
ALTER TABLE [dbo].[Appointments]  WITH CHECK ADD FOREIGN KEY([CreationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Appointments]  WITH CHECK ADD FOREIGN KEY([CustomerID]) REFERENCES [dbo].[Customers] ([ID])
GO
ALTER TABLE [dbo].[Appointments]  WITH CHECK ADD FOREIGN KEY([DeletionUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Appointments]  WITH CHECK ADD FOREIGN KEY([LastModificationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Appointments]  WITH CHECK ADD FOREIGN KEY([ResourceID]) REFERENCES [dbo].[Resources] ([ID])
GO
ALTER TABLE [dbo].[Appointments]  WITH CHECK ADD FOREIGN KEY([StaffID]) REFERENCES [dbo].[Staff] ([ID])
GO
ALTER TABLE [dbo].[Appointments]  WITH CHECK ADD FOREIGN KEY([StatusID]) REFERENCES [dbo].[AppointmentStatus] ([ID])
GO
ALTER TABLE [dbo].[Appointments]  WITH CHECK ADD FOREIGN KEY([TenantID]) REFERENCES [dbo].[Tenants] ([ID])
GO
ALTER TABLE [dbo].[AppointmentServices]  WITH CHECK ADD FOREIGN KEY([AppointmentID]) REFERENCES [dbo].[Appointments] ([ID])
GO
ALTER TABLE [dbo].[AppointmentServices]  WITH CHECK ADD FOREIGN KEY([ServiceID]) REFERENCES [dbo].[Services] ([ID])
GO
ALTER TABLE [dbo].[Availabilities]  WITH CHECK ADD FOREIGN KEY([StaffID]) REFERENCES [dbo].[Staff] ([ID])
GO
ALTER TABLE [dbo].[Availabilities]  WITH CHECK ADD FOREIGN KEY([TenantID]) REFERENCES [dbo].[Tenants] ([ID])
GO
ALTER TABLE [dbo].[ContactInfo]  WITH CHECK ADD FOREIGN KEY([TenantID]) REFERENCES [dbo].[Tenants] ([ID])
GO
ALTER TABLE [dbo].[ContactInfo]  WITH CHECK ADD  CONSTRAINT [FK_ContactInfo_ContactInfoTypes] FOREIGN KEY([TypeID]) REFERENCES [dbo].[ContactInfoTypes] ([ID])
GO
ALTER TABLE [dbo].[Customers]  WITH CHECK ADD FOREIGN KEY([CreationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Customers]  WITH CHECK ADD FOREIGN KEY([DeletionUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Customers]  WITH CHECK ADD FOREIGN KEY([LastModificationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Customers]  WITH CHECK ADD FOREIGN KEY([TenantID]) REFERENCES [dbo].[Tenants] ([ID])
GO
ALTER TABLE [dbo].[Faqs]  WITH CHECK ADD FOREIGN KEY([CategoryID]) REFERENCES [dbo].[Categories] ([ID])
GO
ALTER TABLE [dbo].[JobTypes]  WITH CHECK ADD FOREIGN KEY([CategoryID]) REFERENCES [dbo].[Categories] ([ID])
GO
ALTER TABLE [dbo].[Modules]  WITH CHECK ADD FOREIGN KEY([PlaneID]) REFERENCES [dbo].[Planes] ([ID])
GO
ALTER TABLE [dbo].[Payments]  WITH CHECK ADD FOREIGN KEY([PaymentTypeID]) REFERENCES [dbo].[PaymentTypes] ([ID])
GO
ALTER TABLE [dbo].[Payments]  WITH CHECK ADD FOREIGN KEY([StatusPaymentID]) REFERENCES [dbo].[PaymentStatus] ([ID])
GO
ALTER TABLE [dbo].[Payments]  WITH CHECK ADD FOREIGN KEY([SubscriptionID]) REFERENCES [dbo].[Subscriptions] ([ID])
GO
ALTER TABLE [dbo].[Permissions]  WITH CHECK ADD FOREIGN KEY([PlaneID]) REFERENCES [dbo].[Planes] ([ID])
GO
ALTER TABLE [dbo].[Permissions]  WITH CHECK ADD FOREIGN KEY([SubmoduleID]) REFERENCES [dbo].[Submodules] ([ID])
GO
ALTER TABLE [dbo].[Resources]  WITH CHECK ADD FOREIGN KEY([CreationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Resources]  WITH CHECK ADD FOREIGN KEY([DeletionUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Resources]  WITH CHECK ADD FOREIGN KEY([LastModificationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Resources]  WITH CHECK ADD FOREIGN KEY([TenantID]) REFERENCES [dbo].[Tenants] ([ID])
GO
ALTER TABLE [dbo].[Resources]  WITH CHECK ADD FOREIGN KEY([TypeID]) REFERENCES [dbo].[ResourceTypes] ([ID])
GO
ALTER TABLE [dbo].[ResourceTypes]  WITH CHECK ADD FOREIGN KEY([CategoryID]) REFERENCES [dbo].[Categories] ([ID])
GO
ALTER TABLE [dbo].[RolePermissions]  WITH CHECK ADD FOREIGN KEY([CreationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[RolePermissions]  WITH CHECK ADD FOREIGN KEY([PermissionID]) REFERENCES [dbo].[Permissions] ([ID])
GO
ALTER TABLE [dbo].[RolePermissions]  WITH CHECK ADD FOREIGN KEY([RoleID]) REFERENCES [dbo].[Roles] ([ID])
GO
ALTER TABLE [dbo].[Roles]  WITH CHECK ADD FOREIGN KEY([CreationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Roles]  WITH CHECK ADD FOREIGN KEY([DeletionUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Roles]  WITH CHECK ADD FOREIGN KEY([LastModificationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Services]  WITH CHECK ADD FOREIGN KEY([CategoryID]) REFERENCES [dbo].[Categories] ([ID])
GO
ALTER TABLE [dbo].[Services]  WITH CHECK ADD FOREIGN KEY([CreationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Services]  WITH CHECK ADD FOREIGN KEY([DeletionUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Services]  WITH CHECK ADD FOREIGN KEY([LastModificationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Services]  WITH CHECK ADD FOREIGN KEY([TenantID]) REFERENCES [dbo].[Tenants] ([ID])
GO
ALTER TABLE [dbo].[Settings]  WITH CHECK ADD FOREIGN KEY([SettingTypeID]) REFERENCES [dbo].[SettingTypes] ([ID])
GO
ALTER TABLE [dbo].[Settings]  WITH CHECK ADD FOREIGN KEY([TenantID]) REFERENCES [dbo].[Tenants] ([ID])
GO
ALTER TABLE [dbo].[Staff]  WITH CHECK ADD FOREIGN KEY([CreationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Staff]  WITH CHECK ADD FOREIGN KEY([DeletionUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Staff]  WITH CHECK ADD FOREIGN KEY([JobTypeID]) REFERENCES [dbo].[JobTypes] ([ID])
GO
ALTER TABLE [dbo].[Staff]  WITH CHECK ADD FOREIGN KEY([LastModificationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Staff]  WITH CHECK ADD FOREIGN KEY([TenantID]) REFERENCES [dbo].[Tenants] ([ID])
GO
ALTER TABLE [dbo].[Submodules]  WITH CHECK ADD FOREIGN KEY([CreationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Submodules]  WITH CHECK ADD FOREIGN KEY([ModuleID]) REFERENCES [dbo].[Modules] ([ID])
GO
ALTER TABLE [dbo].[Submodules]  WITH CHECK ADD FOREIGN KEY([PlaneID]) REFERENCES [dbo].[Planes] ([ID])
GO
ALTER TABLE [dbo].[Subscriptions]  WITH CHECK ADD FOREIGN KEY([PlaneID]) REFERENCES [dbo].[Planes] ([ID])
GO
ALTER TABLE [dbo].[Subscriptions]  WITH CHECK ADD FOREIGN KEY([StatusID]) REFERENCES [dbo].[SubscriptionStatus] ([ID])
GO
ALTER TABLE [dbo].[Subscriptions]  WITH CHECK ADD FOREIGN KEY([TenantID]) REFERENCES [dbo].[Tenants] ([ID])
GO
ALTER TABLE [dbo].[TenantFaqs]  WITH CHECK ADD FOREIGN KEY([CreationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[TenantFaqs]  WITH CHECK ADD FOREIGN KEY([DeletionUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[TenantFaqs]  WITH CHECK ADD FOREIGN KEY([FaqID]) REFERENCES [dbo].[Faqs] ([ID])
GO
ALTER TABLE [dbo].[TenantFaqs]  WITH CHECK ADD FOREIGN KEY([LastModificationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[TenantFaqs]  WITH CHECK ADD FOREIGN KEY([TenantID]) REFERENCES [dbo].[Tenants] ([ID])
GO
ALTER TABLE [dbo].[Tenants]  WITH CHECK ADD FOREIGN KEY([DeletionUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Tenants]  WITH CHECK ADD  CONSTRAINT [FK_Tenants_Categories] FOREIGN KEY([CategoryID]) REFERENCES [dbo].[Categories] ([ID])
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_CreationUser] FOREIGN KEY([CreationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_DeletionUser] FOREIGN KEY([DeletionUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_LastModificationUser] FOREIGN KEY([LastModificationUserID]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_Role] FOREIGN KEY([RoleID]) REFERENCES [dbo].[Roles] ([ID])
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_Status] FOREIGN KEY([StatusID]) REFERENCES [dbo].[UserStatuses] ([ID])
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_Tenant] FOREIGN KEY([TenantID]) REFERENCES [dbo].[Tenants] ([ID])
GO
ALTER TABLE [dbo].[SystemLogs]  WITH CHECK ADD FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[SystemLogs]  WITH CHECK ADD FOREIGN KEY([TenantId]) REFERENCES [dbo].[Tenants] ([ID])
GO

USE [master]
GO
ALTER DATABASE [WorkBotAI] SET  READ_WRITE
GO
