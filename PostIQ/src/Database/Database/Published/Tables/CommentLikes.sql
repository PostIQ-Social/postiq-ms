CREATE TABLE [Published].[CommentLikes](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CommentId] [bigint] NOT NULL,
	[UserId] [bigint] NOT NULL,
	[CreatedOn] [datetime2] NOT NULL,
 CONSTRAINT [PK_CommentLikes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Published].[CommentLikes] WITH CHECK ADD CONSTRAINT [FK_CommentLikes_PostComments_CommentId] FOREIGN KEY([CommentId])
REFERENCES [Published].[PostComments] ([Id])
ON DELETE CASCADE
GO

CREATE NONCLUSTERED INDEX [IX_CommentLikes_CommentId] ON [Published].[CommentLikes]
(
	[CommentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
GO
