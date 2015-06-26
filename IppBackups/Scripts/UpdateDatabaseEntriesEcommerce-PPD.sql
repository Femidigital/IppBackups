USE [PPD-Ecommerce]

UPDATE [PPD-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'BPPD'
WHERE [id] = 1

UPDATE [PPD-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'SPPD'
WHERE [id] = 2

UPDATE [PPD-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'SPPD'
WHERE [id] = 3