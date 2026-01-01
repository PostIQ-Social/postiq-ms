CREATE TABLE [Published].[RepoDetails] (
    [RepoDetailsId] BIGINT        IDENTITY (1, 1) NOT NULL,
    [RepoId]        BIGINT        NOT NULL,
    [Key]           VARCHAR (100) NULL,
    [Value]         VARCHAR (MAX) NULL,
    [Ordered]       INT           NULL,
    [IsActive]      VARCHAR (100) NOT NULL,
    [CreatedOn]     DATETIME      NOT NULL,
    [CreatedBy]     BIGINT        NOT NULL,
    [UpdatedOn]     DATETIME      NULL,
    [UpdatedBy]     BIGINT        NULL,
    CONSTRAINT [PK_Publish.RepoDetails] PRIMARY KEY CLUSTERED ([RepoDetailsId] ASC),
    CONSTRAINT [FK_RepoDetails_Repos] FOREIGN KEY ([RepoId]) REFERENCES [Published].[Repos] ([RepoId])
);

