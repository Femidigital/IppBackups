﻿USE [TEST-CloudAdmin]

UPDATE	[TEST-CloudAdmin].dbo.[Credential]
SET		UserName = 'test-cloudadmin',
		[Password] = 'Password1'
WHERE Name = 'admin'

UPDATE	[TEST-CloudAdmin].[dbo].[Credential]
SET		UserName = '363a5e31aa0f4d7894e95d95df875137',
		[Password] = '363a5e31aa0f4d7894e95d95df875137'
WHERE	[id] = 1

UPDATE	[TEST-CloudAdmin].[dbo].[DatabasePool]
SET		Name = 'TEST-DAR'
WHERE	[id] = 3

UPDATE	[TEST-CloudAdmin].[dbo].[DatabasePool]
SET		Name = 'TEST-Eurocodes'
WHERE	[id] = 10

UPDATE	[TEST-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '127.0.0.1',
		Port = 8005
WHERE	[ID] = 5

UPDATE	[TEST-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '127.0.0.1',
		Port = 1433
WHERE	[ID] = 8

UPDATE	[TEST-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '127.0.0.1',
		Port = 8006
WHERE	[ID] = 9

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
WHERE	[ID] = 9

UPDATE	[TEST-CloudAdmin].[dbo].[Product]
SET		Url = 'https://compliancenavigatortest.bsiuat.com'
WHERE	[ID] = 10

UPDATE	[TEST-CloudAdmin].[dbo].[ProductSetting]
SET		Value = 'True'
WHERE	[id] = 5

UPDATE	[TEST-CloudAdmin].[dbo].[TrustedIssuer]
SET		IssuerUri = 'https://identitytest.bsiuat.com'
WHERE	[id] = 1