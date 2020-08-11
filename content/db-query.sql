CREATE TABLE [dbo].[SentBundles] (
	[Id] int NOT NULL IDENTITY(1,1), 
	[BundleId] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
	[SentDate] datetime NOT NULL, 
	[Status] int NOT NULL, 
	[SenderEmail] nvarchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
	[ApiKey] nvarchar(254) COLLATE SQL_Latin1_General_CP1_CI_AS, 
	[CompletedFilePath] nvarchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS, 
	[IsProcessed] bit NOT NULL
)
GO
CREATE NONCLUSTERED INDEX [IX_SentBundles_Status]
	ON [dbo].[SentBundles] ([Status])
	WITH (PAD_INDEX=OFF
	,STATISTICS_NORECOMPUTE=OFF
	,IGNORE_DUP_KEY=OFF
	,ALLOW_ROW_LOCKS=ON
	,ALLOW_PAGE_LOCKS=ON) ON [PRIMARY]