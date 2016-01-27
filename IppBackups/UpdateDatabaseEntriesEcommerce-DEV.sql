USE [DEV-Ecommerce]

UPDATE [DEV-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'BDEV'
WHERE [id] = 1

UPDATE [DEV-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'SDEV'
WHERE [id] = 2

UPDATE [DEV-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'MDEV'
WHERE [id] = 3