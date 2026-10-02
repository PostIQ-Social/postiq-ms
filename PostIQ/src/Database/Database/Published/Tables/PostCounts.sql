CREATE TABLE [Published].[PostsCount](
	[CountId] [bigint] IDENTITY(1,1) NOT NULL,
	[PostId] [bigint] NOT NULL,
	[LikeCount] [int] NOT NULL,
	[CommentCount] [int] NOT NULL,
 CONSTRAINT [PK_Published.PostsCount] PRIMARY KEY CLUSTERED 
(
	[CountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- Foreign key: PostsCount.PostId -> ProcessedPosts.ProcessedPostId
ALTER TABLE [Published].[PostsCount] WITH CHECK ADD CONSTRAINT [FK_Published.PostsCount_Published.ProcessedPosts_PostId]
FOREIGN KEY([PostId])
REFERENCES [Published].[ProcessedPosts] ([ProcessedPostId])
ON DELETE CASCADE
GO

-- Index to support lookups by PostId
CREATE UNIQUE NONCLUSTERED INDEX [IX_Published.PostsCount_PostId] ON [Published].[PostsCount]
(
	[PostId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
GO
