CREATE TABLE [User].[UserDetails] (
    [UserId] BIGINT      IDENTITY (1, 1) NOT NULL,
    [AuthId]       UNIQUEIDENTIFIER       NOT NULL,
    [FirstName]    VARCHAR (50) NOT NULL,
    [MiddleName]   VARCHAR (50) NULL,
    [LastName]     VARCHAR (50) NOT NULL,
    [Phone]        VARCHAR (20) NULL,
    [VerificationCode] VARCHAR (10) NULL,
    [VerificationCodeExpireAt] DATETIME NULL,
    [ReferralCode]  VARCHAR (10) NOT NULL,
    [IsActive]     BIT          NOT NULL,
    [CreatedOn]    DATETIME     NOT NULL,
    [CreatedBy]    BIGINT       NOT NULL,
    [UpdatedOn]    DATETIME     NULL,
    [UpdatedBy]    BIGINT       NULL,
    CONSTRAINT [PK_UserDetails] PRIMARY KEY CLUSTERED ([UserId] ASC)
);

