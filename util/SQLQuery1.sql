/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP (1000) [Path]
      ,[WriteTime]
      ,[LastWriteTime]
      ,[Status]
  FROM [ExportFromFTP].[dbo].[FilesInfo]
  order by 4

--  truncate table [ExportFromFTP].[dbo].[FilesInfo]

-- update [ExportFromFTP].[dbo].[FilesInfo] set Status = 0 where LastWriteTime is null