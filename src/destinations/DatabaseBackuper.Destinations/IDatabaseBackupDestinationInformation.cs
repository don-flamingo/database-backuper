namespace DatabaseBackuper.Destinations;

public interface IDatabaseBackupDestinationInformation
{
    public string Key { get; set; }
    public string Type { get; set; }
}