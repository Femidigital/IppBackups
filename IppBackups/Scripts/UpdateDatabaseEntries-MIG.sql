USE [MIG-CloudAdmin]

UPDATE	[MIG-CloudAdmin].dbo.[Credential]
SET		UserName = 'MIG-cloudadmin',
		[Password] = 'Password1'
WHERE Name = 'admin'

UPDATE	[MIG-CloudAdmin].[dbo].[Credential]
SET		UserName = '363a5e31aa0f4d7894e95d95df875137',
		[Password] = '363a5e31aa0f4d7894e95d95df875137'
WHERE	[id] = 1

UPDATE	[MIG-CloudAdmin].[dbo].[DatabasePool]
SET		Name = 'DAR-MIG'
WHERE	[id] = 3

UPDATE	[MIG-CloudAdmin].[dbo].[DatabasePool]
SET		Name = 'MIG-Eurocodes'
WHERE	[id] = 10

UPDATE	[MIG-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '172.16.87.41',
		Port = 5400
WHERE	[ID] = 5

UPDATE	[MIG-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '172.17.87.42\Cloud',
		Port = 1500
WHERE	[ID] = 8

UPDATE	[MIG-CloudAdmin].[dbo].[Product]
SET		Url = 'https://eurocodesplusqa.bsiuat.com'
WHERE	[ID] = 3

UPDATE	[MIG-CloudAdmin].[dbo].[Product]
SET		Url = 'https://bsolmig.bsiuat.com'
WHERE	[ID] = 4

UPDATE	[MIG-CloudAdmin].[dbo].[Product]
SET		Url = 'https://mcmig.bsiuat.com'
WHERE	[ID] = 6

UPDATE	[MIG-CloudAdmin].[dbo].[Product]
SET		Url = 'https://shopmig.bsiuat.com'
WHERE	[ID] = 8

UPDATE	[MIG-CloudAdmin].[dbo].[Product]
SET		Url = 'https://cclmig.bsiuat.com'
WHERE	[ID] = 10

UPDATE	[MIG-CloudAdmin].[dbo].[ProductSetting]
SET		Value = 'True'
WHERE	[id] = 5

UPDATE	[MIG-CloudAdmin].[dbo].[TrustedIssuer]
SET		IssuerUri = 'https://identityplusmig.bsiuat.com'
WHERE	[id] = 1

/*
INSERT INTO [PPD-CloudAdmin].[dbo].[MonetizationFeatureProduct] (MonetizationFeatureId, ProductId)
VALUES	(28, 4)*/

USE [MIG-PersonalData]

UPDATE	[PPD-PersonalData].[dbo].[Identity]
SET		ClaimIdentifier = REPLACE([ClaimIdentifier], 'https://identity.bsigroup.com', 'https://identityplusmig.bsiuat.com')

USE [MIG-Ecommerce]

UPDATE [MIG-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'BQA'
WHERE [id] = 1

UPDATE [MIG-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'SQA'
WHERE [id] = 2

UPDATE [MIG-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'M'
WHERE [id] = 3


