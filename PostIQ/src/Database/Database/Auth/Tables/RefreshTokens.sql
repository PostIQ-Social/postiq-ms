CREATE TABLE [Auth].[RefreshTokens](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[TokenHash] [nvarchar](512) NOT NULL,
	[ExpiresAt] [datetimeoffset](7) NOT NULL,
	[CreatedAt] [datetimeoffset](7) NOT NULL,
	[RevokedAt] [datetimeoffset](7) NULL,
	[ReplacedByTokenHash] [nvarchar](512) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO