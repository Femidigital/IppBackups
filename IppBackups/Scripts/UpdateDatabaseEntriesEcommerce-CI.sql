USE [CI-Ecommerce]

UPDATE [CI-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'BCI'
WHERE [id] = 1

UPDATE [CI-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'SCI'
WHERE [id] = 2

UPDATE [CI-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'MCI'
WHERE [id] = 3