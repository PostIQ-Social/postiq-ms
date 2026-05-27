CREATE TABLE [Published].[Job] (
    [JobId]       BIGINT        IDENTITY (1, 1) NOT NULL,
    [PublishedId] BIGINT        NOT NULL,
    [UserId]      BIGINT        NOT NULL,
    [Source]      VARCHAR (50)  NOT NULL,
    [BaseUrl]     VARCHAR (100) NOT NULL,
    [IsActive]    BIT NOT NULL,
    [CreatedOn]   DATETIME      NOT NULL,
    [CreatedBy]   BIGINT        NOT NULL,
    [UpdatedOn]   DATETIME      NULL,
    [UpdatedBy]   BIGINT        NULL,
    [ExecutionStartTime]   DATETIME      NULL,
    [NextExecutionTime]   DATETIME      NULL,
    CONSTRAINT [PK_Published.Job] PRIMARY KEY CLUSTERED ([JobId] ASC)
);

