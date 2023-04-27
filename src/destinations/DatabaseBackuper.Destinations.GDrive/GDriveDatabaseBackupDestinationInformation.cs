namespace DatabaseBackuper.Destinations.GDrive;

public class GDriveDatabaseBackupDestinationInformation: IDatabaseBackupDestinationInformation
{
    public string Key { get; set; }
    public string Type { get; set; }
    public string DirectoryId { get; set; }
    public string CredentialsPath { get; set; }
}