﻿USE [PPD-CloudAdmin]

UPDATE	[PPD-CloudAdmin].dbo.[Credential]
SET		UserName = 'PPD-cloudadmin',
		[Password] = '!C0mm4nd1ng$'
WHERE Name = 'admin'

UPDATE	[PPD-CloudAdmin].[dbo].[Credential]
SET		UserName = '525a5e31aa0f4d7894e95d95df875137',
		[Password] = '525a5e31aa0f4d7894e95d95df875137'
WHERE	[id] = 1

UPDATE	[PPD-CloudAdmin].[dbo].[DatabasePool]
SET		Name = 'PPD-DAR'
WHERE	[id] = 3

UPDATE	[PPD-CloudAdmin].[dbo].[DatabasePool]
SET		Name = 'PPD-Eurocodes'
WHERE	[id] = 10

UPDATE	[PPD-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '10.33.201.21',
		Port = 20101
WHERE	[ID] = 5

UPDATE	[PPD-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '10.33.203.33',
		Port = 1433
WHERE	[ID] = 8

UPDATE	[PPD-CloudAdmin].[dbo].[DatabaseNode]
SET		Host = '10.33.207.24',
		Port = 9051
WHERE	[ID] = 9

UPDATE	[PPD-CloudAdmin].[dbo].[Product]
SET		Url = 'https://eurocodesplusppd.bsigroup.com'
WHERE	[ID] = 3

UPDATE	[PPD-CloudAdmin].[dbo].[Product]
SET		Url = 'https://bsolppd.bsigroup.com'
WHERE	[ID] = 4

UPDATE	[PPD-CloudAdmin].[dbo].[Product]
SET		Url = 'https://mcppd.bsigroup.com'
WHERE	[ID] = 6

UPDATE	[PPD-CloudAdmin].[dbo].[Product]
SET		Url = 'https://shopppd.bsigroup.com'
WHERE	[ID] = 8

UPDATE	[PPD-CloudAdmin].[dbo].[Product]
SET		Url = 'https://cclppd.bsigroup.com'
WHERE	[ID] = 9

UPDATE	[PPD-CloudAdmin].[dbo].[Product]
SET		Url = 'https://compliancenavigatorppd.bsigroup.com'
WHERE	[ID] = 10

UPDATE	[PPD-CloudAdmin].[dbo].[ProductSetting]
SET		Value = 'True'
WHERE	[id] = 5

UPDATE	[PPD-CloudAdmin].[dbo].[TrustedIssuer]
SET		IssuerUri = 'https://identityppd.bsigroup.com'
WHERE	[id] = 1
