CREATE TABLE [User].[UserReferral] (
    [ReferralId] BIGINT      IDENTITY (1, 1) NOT NULL,
    [ReferralCode] VARCHAR(10) NOT NULL,
    [UserId]       BIGINT       NOT NULL,
    [UserName]    VARCHAR (100) NOT NULL,
    [ReferredById]   BIGINT       NOT NULL,
    [ReferredByName]    VARCHAR (100) NOT NULL,
    [IsActive]     BIT          NOT NULL,
    [CreatedOn]    DATETIME     NULL,
    [CreatedBy]    BIGINT       NULL,
    CONSTRAINT [PK_UserReferral] PRIMARY KEY CLUSTERED ([ReferralId] ASC)
);

