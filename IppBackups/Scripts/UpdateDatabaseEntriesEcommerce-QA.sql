USE [QA-Ecommerce]

UPDATE [QA-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'BQA'
WHERE [id] = 1

UPDATE [QA-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'SQA'
WHERE [id] = 2

UPDATE [QA-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'M'
WHERE [id] = 3