USE [TEST-Ecommerce]

UPDATE [TEST-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'TESB'
WHERE [id] = 1

UPDATE [TEST-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'TESS'
WHERE [id] = 2

UPDATE [TEST-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'TESM'
WHERE [id] = 3