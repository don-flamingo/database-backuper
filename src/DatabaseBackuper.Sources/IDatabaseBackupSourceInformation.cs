namespace DatabaseBackuper.Sources;

public interface IDatabaseBackupSourceInformation
{
    string Type { get; }
    string DestinationKey { get; }
    string Database { get; }
}