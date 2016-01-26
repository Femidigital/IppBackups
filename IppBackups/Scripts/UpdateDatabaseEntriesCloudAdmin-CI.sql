USE [CI-CloudAdmin]

UPDATE	[CI-CloudAdmin].dbo.[Credential]
SET		UserName = 'ci-cloudadmin',
		[Password] = 'Password1'
WHERE Name = 'admin'

UPDATE	[CI-CloudAdmin].[dbo].[Credential]
SET		UserName = '363a5e31aa0f4d7894e95d95df875137',
		[Password] = '363a5e31aa0f4d7894e95d95df875137'
WHERE	[id] = 1

UPDATE	[CI-CloudAdmin].[dbo].[DatabasePool]
SET		Name = 'DAR-CI'
WHERE	[id] = 3

UPDATE	[CI-CloudAdmin].[dbo].[DatabasePool]
SET		Name = 'CI-Eurocodes'
WHERE	[id] = 10

UPDATE	[CI-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '172.16.87.43',
		Port = 5300
WHERE	[ID] = 5

UPDATE	[CI-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '10.103.109.45',
		Port = 1433
WHERE	[ID] = 8

UPDATE	[CI-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '10.103.109.21',
		Port = 9001
WHERE	[ID] = 9

UPDATE	[CI-CloudAdmin].[dbo].[Product]
SET		Url = 'https://eurocodesplusci.bsiuat.com'
WHERE	[ID] = 3

UPDATE	[CI-CloudAdmin].[dbo].[Product]
SET		Url = 'https://bsolci.bsiuat.com'
WHERE	[ID] = 4

UPDATE	[CI-CloudAdmin].[dbo].[Product]
SET		Url = 'https://mcci.bsiuat.com'
WHERE	[ID] = 6

UPDATE	[CI-CloudAdmin].[dbo].[Product]
SET		Url = 'https://shopci.bsiuat.com'
WHERE	[ID] = 8

UPDATE	[CI-CloudAdmin].[dbo].[Product]
SET		Url = 'https://cclci.bsiuat.com'
WHERE	[ID] = 9

UPDATE	[CI-CloudAdmin].[dbo].[Product]
SET		Url = 'https://compliancenavigatorci.bsiuat.com'
WHERE	[ID] = 10

UPDATE	[CI-CloudAdmin].[dbo].[ProductSetting]
SET		Value = 'True'
WHERE	[id] = 5

UPDATE	[CI-CloudAdmin].[dbo].[TrustedIssuer]
SET		IssuerUri = 'https://identityplusci.bsiuat.com'
WHERE	[id] = 1