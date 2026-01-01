CREATE TABLE [User].[UserDetails] (
    [UserDetailId] BIGINT       NOT NULL,
    [UserId]       BIGINT       NOT NULL,
    [FirstName]    VARCHAR (50) NOT NULL,
    [MiddleName]   VARCHAR (50) NULL,
    [LastName]     VARCHAR (50) NOT NULL,
    [Phone]        VARCHAR (20) NULL,
    [IsActive]     BIT          NOT NULL,
    [CreatedOn]    DATETIME     NOT NULL,
    [CreatedBy]    BIGINT       NOT NULL,
    [UpdatedOn]    DATETIME     NULL,
    [UpdatedBy]    BIGINT       NULL,
    CONSTRAINT [PK_UserDetails] PRIMARY KEY CLUSTERED ([UserDetailId] ASC),
    CONSTRAINT [FK_UserDetails_User] FOREIGN KEY ([UserDetailId]) REFERENCES [User].[Users] ([UserId])
);

