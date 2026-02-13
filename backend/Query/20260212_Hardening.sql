USE [WorkBotAI]
GO

----------------------------------------------------------------------------------------------------
-- ROLES CORRECTION
----------------------------------------------------------------------------------------------------
-- Ensure roles are correctly mapped as per codebase logic
-- 1: SuperAdmin, 2: Admin, 3: Owner, 4: Staff, 5: User

SET IDENTITY_INSERT [dbo].[Roles] ON;

MERGE INTO [dbo].[Roles] AS Target
USING (SELECT 1 AS ID, 'SuperAdmin' AS Name, 1 AS IsActive, 0 AS IsDeleted, GETUTCDATE() AS CreationTime UNION ALL
       SELECT 2, 'Admin', 1, 0, GETUTCDATE() UNION ALL
       SELECT 3, 'Owner', 1, 0, GETUTCDATE() UNION ALL
       SELECT 4, 'Staff', 1, 0, GETUTCDATE() UNION ALL
       SELECT 5, 'User', 1, 0, GETUTCDATE()) AS Source
ON Target.ID = Source.ID
WHEN MATCHED THEN
    UPDATE SET Name = Source.Name, IsActive = Source.IsActive, IsDeleted = Source.IsDeleted
WHEN NOT MATCHED THEN
    INSERT (ID, Name, IsActive, IsDeleted, CreationTime)
    VALUES (Source.ID, Source.Name, Source.IsActive, Source.IsDeleted, Source.CreationTime);

SET IDENTITY_INSERT [dbo].[Roles] OFF;
GO

----------------------------------------------------------------------------------------------------
-- ADMIN USER WITH HASHED PASSWORD
----------------------------------------------------------------------------------------------------
-- Password: Admin123!
-- Hash: $2a$12$SHrCbJXwB0gTRKli9hUM.eK6822UQBCMv5wrarvYUyFBZm2J6Ev3.

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [UserName] = 'admin')
BEGIN
    INSERT INTO [dbo].[Users]
        ([UserName], [Password], [Mail], [StatusID], [FirstName], [LastName], [IsActive], [IsSuperAdmin], [CreationTime], [RoleID])
    VALUES
        ('admin', '$2a$12$SHrCbJXwB0gTRKli9hUM.eK6822UQBCMv5wrarvYUyFBZm2J6Ev3.', 'admin@workbotai.com', 1, 'System', 'Administrator', 1, 1, GETUTCDATE(), 1);
END
ELSE
BEGIN
    UPDATE [dbo].[Users]
    SET [Password] = '$2a$12$SHrCbJXwB0gTRKli9hUM.eK6822UQBCMv5wrarvYUyFBZm2J6Ev3.',
        [RoleID] = 1,
        [IsSuperAdmin] = 1
    WHERE [UserName] = 'admin';
END
GO

----------------------------------------------------------------------------------------------------
-- SYSTEM SETTINGS INITIAL SEED
----------------------------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM [dbo].[SystemSettings])
BEGIN
    INSERT INTO [dbo].[SystemSettings] ([Category], [Key], [Value], [Description], [UpdatedAt])
    VALUES
        ('general', 'platformName', 'WorkBotAI', 'Nome della piattaforma', GETUTCDATE()),
        ('general', 'supportEmail', 'support@workbotai.com', 'Email supporto', GETUTCDATE()),
        ('general', 'defaultLanguage', 'it', 'Lingua predefinita', GETUTCDATE()),
        ('security', 'passwordMinLength', '8', 'Lunghezza minima password', GETUTCDATE()),
        ('appearance', 'primaryColor', '#3B82F6', 'Colore primario', GETUTCDATE());
END
GO
