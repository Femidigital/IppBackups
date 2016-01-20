USE [DEV-CloudAdmin]

UPDATE	[DEV-CloudAdmin].dbo.[Credential]
SET		UserName = 'DEV-cloudadmin',
		[Password] = 'Password1'
WHERE Name = 'admin'

UPDATE	[DEV-CloudAdmin].[dbo].[Credential]
SET		UserName = '363a5e31aa0f4d7894e95d95df875137',
		[Password] = '363a5e31aa0f4d7894e95d95df875137'
WHERE	[id] = 1

UPDATE	[DEV-CloudAdmin].[dbo].[DatabasePool]
SET		Name = 'DEV-DAR'
WHERE	[id] = 3

UPDATE	[DEV-CloudAdmin].[dbo].[DatabasePool]
SET		Name = 'DEV-Eurocodes'
WHERE	[id] = 10

UPDATE	[DEV-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '10.103.109.82',
		Port = 5400
WHERE	[ID] = 5

UPDATE	[DEV-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '10.103.109.45\Cloud',
		Port = 1500
WHERE	[ID] = 8

UPDATE	[DEV-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '10.103.109.21',
		Port = 9001
WHERE	[ID] = 9

UPDATE	[DEV-CloudAdmin].[dbo].[Product]
SET		Url = 'https://eurocodesplusdev.bsiuat.com'
WHERE	[ID] = 3

UPDATE	[DEV-CloudAdmin].[dbo].[Product]
SET		Url = 'https://bsoldev.bsiuat.com'
WHERE	[ID] = 4

UPDATE	[DEV-CloudAdmin].[dbo].[Product]
SET		Url = 'https://mcdev.bsiuat.com'
WHERE	[ID] = 6

UPDATE	[DEV-CloudAdmin].[dbo].[Product]
SET		Url = 'https://shopdev.bsiuat.com'
WHERE	[ID] = 8

UPDATE	[DEV-CloudAdmin].[dbo].[Product]
SET		Url = 'https://ccldev.bsiuat.com'
WHERE	[ID] = 9

UPDATE	[DEV-CloudAdmin].[dbo].[Product]
SET		Url = 'https://compliancenavigatordev.bsiuat.com'
WHERE	[ID] = 10

UPDATE	[DEV-CloudAdmin].[dbo].[ProductSetting]
SET		Value = 'True'
WHERE	[id] = 5

UPDATE	[DEV-CloudAdmin].[dbo].[TrustedIssuer]
SET		IssuerUri = 'https://identityplusdev.bsiuat.com'
WHERE	[id] = 1