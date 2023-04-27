namespace DatabaseBackuper.Exceptions;

public class MissingDatabaseDestinationInformationException: Exception
{
    public string Type { get; }

    public MissingDatabaseDestinationInformationException(string type): base($"Missing database destination information: {type}")
    {
        Type = type;
    }
}