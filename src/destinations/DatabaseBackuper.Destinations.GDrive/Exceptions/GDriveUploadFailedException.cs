namespace DatabaseBackuper.Destinations.GDrive.Exceptions;

public class GDriveUploadFailedException: Exception
{
    public GDriveUploadFailedException(string message): base(message)
    {
        
    }
}