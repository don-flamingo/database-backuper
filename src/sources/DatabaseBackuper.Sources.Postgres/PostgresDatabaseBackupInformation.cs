namespace DatabaseBackuper.Sources.Postgres;

public class PostgresDatabaseBackupInformation: IDatabaseBackupSourceInformation
{
    public string Type => Const.Postgres;
    public string DestinationKey { get; set; }
    
    public int Port { get; set; }
    public string Host { get; set; }
    public string Database { get; set; }
    public string BackupName { get; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string CustomArgs { get; set; }
}