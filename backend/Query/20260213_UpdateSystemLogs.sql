USE [WorkBotAI_N]
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SystemLogs]') AND name = N'UserId')
BEGIN
    ALTER TABLE [dbo].[SystemLogs] ADD [UserId] [int] NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SystemLogs]') AND name = N'RequestJson')
BEGIN
    ALTER TABLE [dbo].[SystemLogs] ADD [RequestJson] [nvarchar](max) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SystemLogs]') AND name = N'ResponseJson')
BEGIN
    ALTER TABLE [dbo].[SystemLogs] ADD [ResponseJson] [nvarchar](max) NULL;
END
GO
