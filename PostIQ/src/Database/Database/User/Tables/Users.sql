CREATE TABLE [User].[Users] (
    [UserId]      BIGINT           IDENTITY (1, 1) NOT NULL,
    [Guid]        UNIQUEIDENTIFIER NOT NULL,
    [Email]       VARCHAR (50)     NOT NULL,
    [OTP]         VARCHAR (10)     NULL,
    [OTPExpireOn] DATETIME         NULL,
    [IsActive]    BIT              NOT NULL,
    [CreatedOn]   DATETIME         NULL,
    [CreatedBy]   BIGINT           NULL,
    [UpdatedOn]   DATETIME         NULL,
    [UpdatedBy]   BIGINT           NULL,
    [IpAddress]   VARCHAR (50)     NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([UserId] ASC)
);

