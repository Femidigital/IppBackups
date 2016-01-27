USE [TEST-Ecommerce]

UPDATE [TEST-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'BDEV'
WHERE [id] = 1

UPDATE [TEST-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'SDEV'
WHERE [id] = 2

UPDATE [TEST-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'MDEV'
WHERE [id] = 3