namespace DatabaseBackuper.Destinations;

public interface IDatabaseBackupDestination
{
    public string Type { get; }

    Task UploadBackupAsync(string filePath, IDatabaseBackupDestinationInformation databaseBackupDestinationInformation, CancellationToken cancellationToken);
    
    Task DeletePreviousBackupsAsync(TimeSpan olderThan, IDatabaseBackupDestinationInformation databaseBackupDestinationInformation, CancellationToken cancellationToken);
}