$sqlconn = New-Object System.Data.SqlClient.SqlConnection
$sqlconn.ConnectionString = "Server=(localdb)\mssqllocaldb;Database=ExportFromFTP;Integrated Security=True"
$sqlconn.Open()

$query = "truncate table [ExportFromFTP].[dbo].[FilesInfo]"
$sqlcmd = $sqlconn.CreateCommand()
$sqlcmd.CommandText = $query
$sqlcmd.ExecuteNonQuery()

dotnet run