CREATE TABLE [Published].[Repos] (
    [RepoId]      BIGINT        IDENTITY (1, 1) NOT NULL,
    [JobId]       BIGINT        NOT NULL,
    [PublishedId] BIGINT        NOT NULL,
    [Source]      VARCHAR (50)  NOT NULL,
    [RepoUrl]     VARCHAR (100) NOT NULL,
    [Status]      VARCHAR (10)  NOT NULL,
    [IsActive]    VARCHAR (100) NOT NULL,
    [PostedOn]    DATETIME      NOT NULL,
    [CreatedOn]   DATETIME      NOT NULL,
    [CreatedBy]   BIGINT        NOT NULL,
    [UpdatedOn]   DATETIME      NULL,
    [UpdatedBy]   BIGINT        NULL,
    CONSTRAINT [PK_Published_Repo.Job] PRIMARY KEY CLUSTERED ([RepoId] ASC),
    CONSTRAINT [FK_Repos_Job] FOREIGN KEY ([JobId]) REFERENCES [Published].[Job] ([JobId])
);

