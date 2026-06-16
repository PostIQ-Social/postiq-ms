CREATE TABLE [Home].[PostComments](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PostId] [bigint] NOT NULL,
	[UserId] [bigint] NOT NULL,
	[Content] [nvarchar](1000) NOT NULL,
	[CreatedOn] [datetime2] NOT NULL,
	[ParentCommentId] [bigint] NULL,
	[LikeCount] [int] NOT NULL DEFAULT 0,
 CONSTRAINT [PK_PostComments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [Home].[PostComments] WITH CHECK ADD CONSTRAINT [FK_PostComments_Posts_PostId] FOREIGN KEY([PostId])
REFERENCES [Home].[Posts] ([Id])
ON DELETE CASCADE

ALTER TABLE [Home].[PostComments] WITH CHECK ADD CONSTRAINT [FK_PostComments_PostComments_ParentCommentId] FOREIGN KEY([ParentCommentId])
REFERENCES [Home].[PostComments] ([Id])

CREATE NONCLUSTERED INDEX [IX_PostComments_PostId] ON [Home].[PostComments]
(
	[PostId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_PostComments_ParentCommentId] ON [Home].[PostComments]
(
	[ParentCommentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
