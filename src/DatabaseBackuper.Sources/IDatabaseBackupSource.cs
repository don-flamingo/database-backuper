namespace DatabaseBackuper.Sources;

public interface IDatabaseBackupSource
{
    string Type { get; }

    string GetFileNameWithExtension(string fileName);
    
    Task BackupAsync(string toFile, 
        IDatabaseBackupSourceInformation information,
        CancellationToken cancellationToken);
}