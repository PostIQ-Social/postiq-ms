CREATE TABLE [Auth].[User](
	[Id] [uniqueidentifier] NOT NULL,
	[Email] [nvarchar](320) NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[PasswordHash] [nvarchar](512) NOT NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PhoneNumber] [nvarchar](32) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[TwoFactorSecret] [nvarchar](512) NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[AccessFailedCount] [int] NOT NULL,
	[Roles] [nvarchar](256) NOT NULL,
	[CreatedAt] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO