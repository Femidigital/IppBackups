USE [MIG-Ecommerce]

UPDATE [MIG-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'BQA'
WHERE [id] = 1

UPDATE [MIG-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'SQA'
WHERE [id] = 2

UPDATE [MIG-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'M'
WHERE [id] = 3
