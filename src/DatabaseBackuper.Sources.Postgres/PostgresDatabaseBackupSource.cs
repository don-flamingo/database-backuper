using System.Diagnostics;
using DatabaseBackuper.Framework;
using Serilog;

namespace DatabaseBackuper.Sources.Postgres;

public class PostgresDatabaseBackupSource: IDatabaseBackupSource
{
    private readonly ILogger _logger;
    
    public string Type => Const.Postgres;

    public PostgresDatabaseBackupSource(ILogger logger)
    {
        _logger = logger;
    }

    public string GetFileNameWithExtension(string fileName)
    {
        return $"{fileName}.sql";
    }

    public async Task BackupAsync(
        string toFile, 
        IDatabaseBackupSourceInformation information, 
        CancellationToken cancellationToken)
    {
        var pgInformation = (PostgresDatabaseBackupInformation) information;
        var cmd = @$"PGPASSWORD=""{pgInformation.Password}"" pg_dump";
        var args = $"{cmd} -h {pgInformation.Host} -p {pgInformation.Port} -U {pgInformation.Username} -d {pgInformation.Database} {pgInformation.CustomArgs} > {toFile}";

        await ShellHelper.Bash(args, _logger);
    }
}