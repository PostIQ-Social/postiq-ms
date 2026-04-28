CREATE TABLE [Auth].[SecurityTokens](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[Kind] [int] NOT NULL,
	[TokenHash] [nvarchar](512) NOT NULL,
	[ExpiresAt] [datetimeoffset](7) NOT NULL,
	[IsUsed] [bit] NOT NULL,
	[CreatedAt] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


