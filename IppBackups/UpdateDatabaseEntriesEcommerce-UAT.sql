USE [UAT-Ecommerce]

UPDATE [UAT-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'BUAT'
WHERE [id] = 1

UPDATE [UAT-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'SUAT'
WHERE [id] = 2

UPDATE [UAT-Ecommerce].[dbo].[Application]
SET ApplicationPrefix = 'MUAT'
WHERE [id] = 3