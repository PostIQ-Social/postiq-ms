CREATE TABLE [User].[Posts] (
    [Id]        BIGINT        IDENTITY (1, 1) NOT NULL,
    [UserId]    BIGINT        NOT NULL,
    [Source]    VARCHAR (50)  NULL,
    [RepoUrl]   VARCHAR (100) NULL,
    [Key]       VARCHAR (100) NULL,
    [Value]     VARCHAR (MAX) NULL,
    [Ordered]   INT           NULL,
    [IsActive]  VARCHAR (100) NOT NULL,
    [PostedOn]  DATETIME      NOT NULL,
    [CreatedOn] DATETIME      NOT NULL,
    [CreatedBy] BIGINT        NOT NULL,
    [UpdatedOn] DATETIME      NULL,
    [UpdatedBy] BIGINT        NULL,
    CONSTRAINT [PK_User.Posts] PRIMARY KEY CLUSTERED ([Id] ASC)
);

