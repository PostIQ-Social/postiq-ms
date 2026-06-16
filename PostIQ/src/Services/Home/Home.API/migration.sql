BEGIN TRANSACTION;
ALTER TABLE [Home].[Posts] ADD [CommentCount] int NOT NULL DEFAULT 0;

ALTER TABLE [Home].[Posts] ADD [LikeCount] int NOT NULL DEFAULT 0;

CREATE TABLE [Home].[PostComments] (
    [Id] bigint NOT NULL IDENTITY,
    [PostId] bigint NOT NULL,
    [UserId] bigint NOT NULL,
    [Content] nvarchar(1000) NOT NULL,
    [CreatedOn] datetime2 NOT NULL,
    CONSTRAINT [PK_PostComments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PostComments_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Home].[Posts] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Home].[PostLikes] (
    [Id] bigint NOT NULL IDENTITY,
    [PostId] bigint NOT NULL,
    [UserId] bigint NOT NULL,
    [CreatedOn] datetime2 NOT NULL,
    CONSTRAINT [PK_PostLikes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PostLikes_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Home].[Posts] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_PostComments_PostId] ON [Home].[PostComments] ([PostId]);

CREATE INDEX [IX_PostLikes_PostId] ON [Home].[PostLikes] ([PostId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260513191101_AddLikeAndCommentFeatures', N'10.0.8');

COMMIT;
GO

