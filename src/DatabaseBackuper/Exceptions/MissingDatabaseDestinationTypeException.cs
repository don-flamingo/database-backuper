namespace DatabaseBackuper.Exceptions;

public class MissingDatabaseDestinationTypeException: Exception
{
    public string Type { get; }

    public MissingDatabaseDestinationTypeException(string type): base($"Missing database destination type: {type}")
    {
        Type = type;
    }
}