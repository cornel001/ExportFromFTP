$sqlconn = New-Object System.Data.SqlClient.SqlConnection
$sqlconn.ConnectionString = "Server=(localdb)\mssqllocaldb;Database=ExportFromFTP;Integrated Security=True"
$sqlconn.Open()
$sqlcmd = $sqlconn.CreateCommand()

$query = "truncate table [ExportFromFTP].[dbo].[FilesInfo]"
$sqlcmd.CommandText = $query
$sqlcmd.ExecuteNonQuery()

$query = "select count(*) from ExportFromFTP.dbo.FilesInfo"
$sqlcmd.CommandText = $query
$count = $sqlcmd.ExecuteScalar()
Write-Host "Count after truncate:" $count $(if ($count) {"FAILED"} else {"PASSED"})


dotnet run