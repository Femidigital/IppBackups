USE [UAT-CloudAdmin]

UPDATE	[UAT-CloudAdmin].dbo.[Credential]
SET		UserName = 'uat-cloudadmin',
		[Password] = 'Password1'
WHERE Name = 'admin'

UPDATE	[UAT-CloudAdmin].[dbo].[Credential]
SET		UserName = '363a5e31aa0f4d7894e95d95df875137',
		[Password] = '363a5e31aa0f4d7894e95d95df875137'
WHERE	[id] = 1

UPDATE	[UAT-CloudAdmin].[dbo].[DatabasePool]
SET		Name = 'UAT-DAR'
WHERE	[id] = 3

UPDATE	[UAT-CloudAdmin].[dbo].[DatabasePool]
SET		Name = 'UAT-Eurocodes'
WHERE	[id] = 10

UPDATE	[UAT-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '172.16.87.41',
		Port = 5500
WHERE	[ID] = 5

UPDATE	[UAT-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '172.16.87.42',
		Port = 1500
WHERE	[ID] = 8

UPDATE	[UAT-CloudAdmin].[dbo].[Product]
SET		Url = 'https://infrastructure.local.bsiuat.com'
WHERE	[ID] = 1

UPDATE	[UAT-CloudAdmin].[dbo].[Product]
SET		Url = 'https://platform.local.bsiuat.com'
WHERE	[ID] = 2

UPDATE	[UAT-CloudAdmin].[dbo].[Product]
SET		Url = 'https://eurocodesplus.bsiuat.com'
WHERE	[ID] = 3

UPDATE	[UAT-CloudAdmin].[dbo].[Product]
SET		Url = 'https://bsoluat.bsiuat.com'
WHERE	[ID] = 4

UPDATE	[UAT-CloudAdmin].[dbo].[Product]
SET		Url = 'https://dar.local.bsiuat.com'
WHERE	[ID] = 5

UPDATE	[UAT-CloudAdmin].[dbo].[Product]
SET		Url = 'https://mc.bsiuat.com'
WHERE	[ID] = 6

UPDATE	[UAT-CloudAdmin].[dbo].[Product]
SET		Url = 'https://ecommerce.local.bsiuat.com'
WHERE	[ID] = 7

UPDATE	[UAT-CloudAdmin].[dbo].[Product]
SET		Url = 'https://shopuat.bsiuat.com'
WHERE	[ID] = 8

UPDATE	[UAT-CloudAdmin].[dbo].[Product]
SET		Url = 'https://ccl.bsiuat.com'
WHERE	[ID] = 9

UPDATE	[UAT-CloudAdmin].[dbo].[Product]
SET		Url = 'https://compliancenavigator.bsiuat.com'
WHERE	[ID] = 10

UPDATE	[UAT-CloudAdmin].[dbo].[ProductSetting]
SET		Value = 'True'
WHERE	[id] = 5

UPDATE	[UAT-CloudAdmin].[dbo].[TrustedIssuer]
SET		IssuerUri = 'https://identityuat.bsiuat.com'
WHERE	[id] = 1
