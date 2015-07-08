
USE [TEST-PersonalData]

DECLARE @currentIdentity VARCHAR(50)
SET @currentIdentity = (SELECT TOP 1 ClaimIdentifier FROM dbo.[Identity] (NOLOCK)
Order by [Id] ASC)

SELECT @currentIdentity = SUBSTRING(@currentIdentity,0,PATINDEX('%.com%',@currentIdentity))

UPDATE	[TEST-PersonalData].[dbo].[Identity]
SET		ClaimIdentifier = REPLACE([ClaimIdentifier], '@currentIdentity', 'https://identityplustest')