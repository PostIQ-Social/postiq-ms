CREATE TABLE [User].[Published] (
    [PublishedId] BIGINT        NOT NULL,
    [UserId]      BIGINT        NOT NULL,
    [Source]      VARCHAR (50)  NULL,
    [BaseUrl]     VARCHAR (20)  NULL,
    [IsActive]    VARCHAR (100) NOT NULL,
    [CreatedOn]   DATETIME      NOT NULL,
    [CreatedBy]   BIGINT        NOT NULL,
    [UpdatedOn]   DATETIME      NULL,
    [UpdatedBy]   BIGINT        NULL,
    CONSTRAINT [PK_Published] PRIMARY KEY CLUSTERED ([PublishedId] ASC),
    CONSTRAINT [FK_Published_User] FOREIGN KEY ([UserId]) REFERENCES [User].[Users] ([UserId])
);

