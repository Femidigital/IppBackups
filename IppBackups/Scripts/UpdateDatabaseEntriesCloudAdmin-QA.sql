USE [QA-CloudAdmin]

UPDATE	[QA-CloudAdmin].dbo.[Credential]
SET		UserName = 'qa-cloudadmin',
		[Password] = 'Password1'
WHERE Name = 'admin'

UPDATE	[QA-CloudAdmin].[dbo].[Credential]
SET		UserName = '363a5e31aa0f4d7894e95d95df875137',
		[Password] = '363a5e31aa0f4d7894e95d95df875137'
WHERE	[id] = 1

UPDATE	[QA-CloudAdmin].[dbo].[DatabasePool]
SET		Name = 'DAR-QA'
WHERE	[id] = 3

UPDATE	[QA-CloudAdmin].[dbo].[DatabasePool]
SET		Name = 'QA-Eurocodes'
WHERE	[id] = 10

UPDATE	[QA-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '172.16.87.43',
		Port = 5400
WHERE	[ID] = 5

UPDATE	[QA-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '10.103.109.45',
		Port = 1433
WHERE	[ID] = 8

UPDATE	[QA-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '10.103.109.21',
		Port = 9001
WHERE	[ID] = 9

UPDATE	[QA-CloudAdmin].[dbo].[Product]
SET		Url = 'https://eurocodesplusqa.bsiuat.com'
WHERE	[ID] = 3

UPDATE	[QA-CloudAdmin].[dbo].[Product]
SET		Url = 'https://bsolqa.bsiuat.com'
WHERE	[ID] = 4

UPDATE	[QA-CloudAdmin].[dbo].[Product]
SET		Url = 'https://mcqa.bsiuat.com'
WHERE	[ID] = 6

UPDATE	[QA-CloudAdmin].[dbo].[Product]
SET		Url = 'https://shopqa.bsiuat.com'
WHERE	[ID] = 8

UPDATE	[QA-CloudAdmin].[dbo].[Product]
SET		Url = 'https://cclqa.bsiuat.com'
WHERE	[ID] = 10

UPDATE	[QA-CloudAdmin].[dbo].[ProductSetting]
SET		Value = 'True'
WHERE	[id] = 5

UPDATE	[QA-CloudAdmin].[dbo].[TrustedIssuer]
SET		IssuerUri = 'https://identityplusqa.bsiuat.com'
WHERE	[id] = 1
