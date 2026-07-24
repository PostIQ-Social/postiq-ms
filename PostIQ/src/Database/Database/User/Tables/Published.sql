CREATE TABLE [User].[Published] (
    [PublishedId] BIGINT        IDENTITY (1, 1) NOT NULL,
    [UserId]      BIGINT        NOT NULL,
    [Source]      VARCHAR (50)  NULL,
    [BaseUrl]     VARCHAR (200) NULL,
    [IsActive]    BIT           NOT NULL,
    [CreatedOn]   DATETIME      NOT NULL,
    [CreatedBy]   BIGINT        NOT NULL,
    [UpdatedOn]   DATETIME      NULL,
    [UpdatedBy]   BIGINT        NULL,
    CONSTRAINT [PK_Published] PRIMARY KEY CLUSTERED ([PublishedId] ASC),
);

