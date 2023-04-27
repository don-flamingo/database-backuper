namespace DatabaseBackuper.Exceptions;

public class MissingDatabaseSourceTypeException: Exception
{
    public string Type { get; }

    public MissingDatabaseSourceTypeException(string type): base($"Missing database source type: {type}")
    {
        Type = type;
    }
}