SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @AdminAuthId UNIQUEIDENTIFIER;
DECLARE @AdminUserId BIGINT;
DECLARE @InsertedUser TABLE (UserId BIGINT);

SELECT @AdminAuthId = [Id]
FROM [Auth].[User]
WHERE [Email] = N'admin@postiq.com';

IF @AdminAuthId IS NULL
BEGIN
    SET @AdminAuthId = 'd4d56ce1-41ad-4b04-9a7e-fc9d9ac1a001';

    INSERT INTO [Auth].[User]
    (
        [Id], [Email], [UserName], [PasswordHash], [EmailConfirmed],
        [PhoneNumberConfirmed], [TwoFactorEnabled], [AccessFailedCount], [Roles], [CreatedAt]
    )
    VALUES
    (
        @AdminAuthId,
        N'admin@postiq.com',
        N'Admin user',
        N'AQ2KJmZ+z39dUnT6qjykv3MKHHesT+cIoNKfxoaQOU0RE8uI8eZ7Xj18vHGMJNthQw==',
        1, 0, 0, 0, N'User,Admin', SYSUTCDATETIME()
    );
END
ELSE
BEGIN
    UPDATE [Auth].[User]
    SET [UserName] = N'Admin user',
        [Roles] = CASE
            WHEN N',' + REPLACE(ISNULL([Roles], N''), N' ', N'') + N',' LIKE N'%,Admin,%' THEN [Roles]
            WHEN NULLIF(LTRIM(RTRIM([Roles])), N'') IS NULL THEN N'User,Admin'
            ELSE [Roles] + N',Admin'
        END
    WHERE [Id] = @AdminAuthId;
END;

IF NOT EXISTS (SELECT 1 FROM [User].[UserDetails] WHERE [AuthId] = @AdminAuthId)
BEGIN
    IF EXISTS (SELECT 1 FROM [User].[UserDetails] WHERE [ReferralCode] = '123456')
        THROW 50001, 'Referral code 123456 is already assigned to another user.', 1;

    INSERT INTO [User].[UserDetails]
    (
        [AuthId], [FirstName], [LastName], [ReferralCode], [IsActive], [CreatedOn], [CreatedBy]
    )
    OUTPUT inserted.[UserId] INTO @InsertedUser ([UserId])
    VALUES
    (
        @AdminAuthId, 'Admin', 'user', '123456', 1, GETUTCDATE(), 0
    );

    SELECT @AdminUserId = [UserId] FROM @InsertedUser;

    UPDATE [User].[UserDetails]
    SET [CreatedBy] = @AdminUserId
    WHERE [UserId] = @AdminUserId;
END;

COMMIT TRANSACTION;