CREATE TABLE [Home].[BatchJobStatus](
	[StatusId] [bigint] IDENTITY(1,1) NOT NULL,
	[BatchId] UNIQUEIDENTIFIER NOT NULL,
	[BatchSize] [int] NOT NULL,
	[StartId] [bigint] NOT NULL,
	[LastId] [bigint] NOT NULL,
	[RecordCount] [int] NOT NULL,
	[ExecutionStartedAt] [datetime] NULL,
	[ExecutionEndedAt] [datetime] NULL,
	[Status] [varchar](10) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [bigint] NOT NULL,
 CONSTRAINT [PK_SyncJob] PRIMARY KEY CLUSTERED 
(
	[StatusId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]