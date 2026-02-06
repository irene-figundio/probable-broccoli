USE [WorkBotAI]
GO

----------------------------------------------------------------------------------------------------
-- SEED DATA
----------------------------------------------------------------------------------------------------

-- User Statuses
INSERT INTO [dbo].[UserStatuses] ([Name], [IsActive]) VALUES ('Active', 1);
INSERT INTO [dbo].[UserStatuses] ([Name], [IsActive]) VALUES ('Inactive', 1);
INSERT INTO [dbo].[UserStatuses] ([Name], [IsActive]) VALUES ('Pending', 1);
INSERT INTO [dbo].[UserStatuses] ([Name], [IsActive]) VALUES ('Suspended', 1);
GO

-- Roles
INSERT INTO [dbo].[Roles] ([Name], [IsActive]) VALUES ('SuperAdmin', 1);
INSERT INTO [dbo].[Roles] ([Name], [IsActive]) VALUES ('Admin', 1);
INSERT INTO [dbo].[Roles] ([Name], [IsActive]) VALUES ('Owner', 1);
INSERT INTO [dbo].[Roles] ([Name], [IsActive]) VALUES ('Staff', 1);
INSERT INTO [dbo].[Roles] ([Name], [IsActive]) VALUES ('User', 1);
GO

-- Appointment Status
INSERT INTO [dbo].[AppointmentStatus] ([Name], [IsActive]) VALUES ('Scheduled', 1);
INSERT INTO [dbo].[AppointmentStatus] ([Name], [IsActive]) VALUES ('Confirmed', 1);
INSERT INTO [dbo].[AppointmentStatus] ([Name], [IsActive]) VALUES ('Completed', 1);
INSERT INTO [dbo].[AppointmentStatus] ([Name], [IsActive]) VALUES ('Cancelled', 1);
INSERT INTO [dbo].[AppointmentStatus] ([Name], [IsActive]) VALUES ('No Show', 1);
GO

-- Categories
INSERT INTO [dbo].[Categories] ([Name], [IsActive]) VALUES ('Health & Wellness', 1);
INSERT INTO [dbo].[Categories] ([Name], [IsActive]) VALUES ('Beauty & Hair', 1);
INSERT INTO [dbo].[Categories] ([Name], [IsActive]) VALUES ('Professional Services', 1);
INSERT INTO [dbo].[Categories] ([Name], [IsActive]) VALUES ('Education', 1);
INSERT INTO [dbo].[Categories] ([Name], [IsActive]) VALUES ('Sports & Fitness', 1);
GO

-- Contact Info Types
INSERT INTO [dbo].[ContactInfoTypes] ([Name]) VALUES ('Email');
INSERT INTO [dbo].[ContactInfoTypes] ([Name]) VALUES ('Phone');
INSERT INTO [dbo].[ContactInfoTypes] ([Name]) VALUES ('Mobile');
INSERT INTO [dbo].[ContactInfoTypes] ([Name]) VALUES ('WhatsApp');
INSERT INTO [dbo].[ContactInfoTypes] ([Name]) VALUES ('Website');
GO

-- Payment Status
INSERT INTO [dbo].[PaymentStatus] ([Name], [IsActive]) VALUES ('Pending', 1);
INSERT INTO [dbo].[PaymentStatus] ([Name], [IsActive]) VALUES ('Completed', 1);
INSERT INTO [dbo].[PaymentStatus] ([Name], [IsActive]) VALUES ('Failed', 1);
INSERT INTO [dbo].[PaymentStatus] ([Name], [IsActive]) VALUES ('Refunded', 1);
GO

-- Payment Types
INSERT INTO [dbo].[PaymentTypes] ([ID], [Name], [IsActive], [BaseGateway]) VALUES (1, 'Cash', 1, NULL);
INSERT INTO [dbo].[PaymentTypes] ([ID], [Name], [IsActive], [BaseGateway]) VALUES (2, 'Credit Card', 1, 'Nexi');
INSERT INTO [dbo].[PaymentTypes] ([ID], [Name], [IsActive], [BaseGateway]) VALUES (3, 'PayPal', 1, 'PayPal');
INSERT INTO [dbo].[PaymentTypes] ([ID], [Name], [IsActive], [BaseGateway]) VALUES (4, 'Bank Transfer', 1, NULL);
GO

-- Planes
INSERT INTO [dbo].[Planes] ([Name], [IsActive], [Description]) VALUES ('Free', 1, 'Piano gratuito limitato');
INSERT INTO [dbo].[Planes] ([Name], [IsActive], [Description]) VALUES ('Basic', 1, 'Piano base per piccole attività');
INSERT INTO [dbo].[Planes] ([Name], [IsActive], [Description]) VALUES ('Premium', 1, 'Piano completo per professionisti');
INSERT INTO [dbo].[Planes] ([Name], [IsActive], [Description]) VALUES ('Enterprise', 1, 'Soluzione personalizzata per grandi aziende');
GO

-- Subscription Status
INSERT INTO [dbo].[SubscriptionStatus] ([Name], [IsActive]) VALUES ('Active', 1);
INSERT INTO [dbo].[SubscriptionStatus] ([Name], [IsActive]) VALUES ('Trial', 1);
INSERT INTO [dbo].[SubscriptionStatus] ([Name], [IsActive]) VALUES ('Expired', 1);
INSERT INTO [dbo].[SubscriptionStatus] ([Name], [IsActive]) VALUES ('Cancelled', 1);
GO

-- Setting Types
INSERT INTO [dbo].[SettingTypes] ([Name]) VALUES ('General');
INSERT INTO [dbo].[SettingTypes] ([Name]) VALUES ('Email');
INSERT INTO [dbo].[SettingTypes] ([Name]) VALUES ('Notifications');
INSERT INTO [dbo].[SettingTypes] ([Name]) VALUES ('Appearance');
INSERT INTO [dbo].[SettingTypes] ([Name]) VALUES ('Security');
GO

-- System Settings
INSERT INTO [dbo].[SystemSettings] ([Category], [Key], [Value], [Description]) VALUES ('general', 'platformName', 'WorkBotAI', 'Nome della piattaforma');
INSERT INTO [dbo].[SystemSettings] ([Category], [Key], [Value], [Description]) VALUES ('general', 'supportEmail', 'support@workbotai.com', 'Email supporto');
INSERT INTO [dbo].[SystemSettings] ([Category], [Key], [Value], [Description]) VALUES ('general', 'defaultLanguage', 'it', 'Lingua predefinita');
INSERT INTO [dbo].[SystemSettings] ([Category], [Key], [Value], [Description]) VALUES ('general', 'timezone', 'Europe/Rome', 'Fuso orario');
INSERT INTO [dbo].[SystemSettings] ([Category], [Key], [Value], [Description]) VALUES ('appearance', 'primaryColor', '#3B82F6', 'Colore primario');
INSERT INTO [dbo].[SystemSettings] ([Category], [Key], [Value], [Description]) VALUES ('security', 'passwordMinLength', '8', 'Lunghezza minima password');
INSERT INTO [dbo].[SystemSettings] ([Category], [Key], [Value], [Description]) VALUES ('security', 'requireSpecialChar', 'true', 'Richiedi caratteri speciali');
GO

-- Default SuperAdmin User (Password: Admin123!)
-- Note: In production, password should be hashed.
INSERT INTO [dbo].[Users]
    ([UserName], [Password], [Mail], [StatusID], [FirstName], [LastName], [IsActive], [IsSuperAdmin], [CreationTime])
VALUES
    ('admin', 'Admin123!', 'admin@workbotai.com', 1, 'System', 'Administrator', 1, 1, GETUTCDATE());
GO
