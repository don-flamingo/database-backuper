using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;

namespace DatabaseBackuper.Destinations.GDrive;

public class GDriveDatabaseBackupDestination: IDatabaseBackupDestination
{
    public string Type => Const.Type;

    public async Task UploadBackupAsync(
        string filePath, 
        IDatabaseBackupDestinationInformation databaseBackupDestinationInformation,
        CancellationToken cancellationToken)
    {
        var gDriveInformation = (GDriveDatabaseBackupDestinationInformation) databaseBackupDestinationInformation;
        var credential = GoogleCredential.FromFile(gDriveInformation.CredentialsPath)
            .CreateScoped(DriveService.ScopeConstants.Drive);
        
        var service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential
        });

        var fileName = Path.GetFileName(filePath);
        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = fileName,
            Parents = new List<string> { gDriveInformation.DirectoryId }
        };
        
        await using var stream = new FileStream(filePath, FileMode.Open);
        
        var request = service.Files.Create(fileMetadata, stream, "text/plain");
        request.Fields = "*";
        
        var results = await request.UploadAsync(cancellationToken);
        if (results.Status != UploadStatus.Completed)
        {
            throw new Exception($"Failed to upload file: {results.Exception.Message}");
        }
    }

    public Task DeletePreviousBackupsAsync(
        TimeSpan olderThan,
        IDatabaseBackupDestinationInformation databaseBackupDestinationInformation, 
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}