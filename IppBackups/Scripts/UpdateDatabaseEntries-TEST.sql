USE [TEST-CloudAdmin]

UPDATE	[TEST-CloudAdmin].dbo.[Credential]
SET		UserName = 'test-cloudadmin',
		[Password] = 'Password1'
WHERE Name = 'admin'

UPDATE	[TEST-CloudAdmin].[dbo].[Credential]
SET		UserName = '363a5e31aa0f4d7894e95d95df875137',
		[Password] = '363a5e31aa0f4d7894e95d95df875137'
WHERE	[id] = 1

UPDATE	[TEST-CloudAdmin].[dbo].[DatabasePool]
SET		Name = 'DAR-TEST'
WHERE	[id] = 3

UPDATE	[TEST-CloudAdmin].[dbo].[DatabasePool]
SET		Name = 'TEST-Eurocodes'
WHERE	[id] = 10

UPDATE	[TEST-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '172.16.87.41',
		Port = 5400
WHERE	[ID] = 5

UPDATE	[TEST-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '172.17.87.42\Cloud',
		Port = 1500
WHERE	[ID] = 8

UPDATE	[TEST-CloudAdmin].[dbo].[Product]
SET		Url = 'https://eurocodesplustest.bsiuat.com'
WHERE	[ID] = 3

UPDATE	[TEST-CloudAdmin].[dbo].[Product]
SET		Url = 'https://bsoltest.bsiuat.com'
WHERE	[ID] = 4

UPDATE	[TEST-CloudAdmin].[dbo].[Product]
SET		Url = 'https://mctest.bsiuat.com'
WHERE	[ID] = 6

UPDATE	[TEST-CloudAdmin].[dbo].[Product]
SET		Url = 'https://shoptest.bsiuat.com'
WHERE	[ID] = 8

UPDATE	[TEST-CloudAdmin].[dbo].[Product]
SET		Url = 'https://ccltest.bsiuat.com'
WHERE	[ID] = 10

UPDATE	[TEST-CloudAdmin].[dbo].[ProductSetting]
SET		Value = 'True'
WHERE	[id] = 5

UPDATE	[TEST-CloudAdmin].[dbo].[TrustedIssuer]
SET		IssuerUri = 'https://identityplustest.bsiuat.com'
WHERE	[id] = 1

/*
INSERT INTO [PPD-CloudAdmin].[dbo].[MonetizationFeatureProduct] (MonetizationFeatureId, ProductId)
VALUES	(28, 4)*/

USE [TEST-PersonalData]

UPDATE	[PPD-PersonalData].[dbo].[Identity]
SET		ClaimIdentifier = REPLACE([ClaimIdentifier], 'https://identity.bsigroup.com', 'https://identityplustest.bsiuat.com')

USE [TEST-Ecommerce]

UPDATE [TEST-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'BTS'
WHERE [id] = 1

UPDATE [TEST-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'STS'
WHERE [id] = 2

UPDATE [TEST-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'MTS'
WHERE [id] = 3


