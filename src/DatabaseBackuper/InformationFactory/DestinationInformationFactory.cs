using DatabaseBackuper.Destinations;
using DatabaseBackuper.Destinations.GDrive;
using DatabaseBackuper.Sources;

namespace DatabaseBackuper.InformationFactory;

public static class DestinationInformationFactory
{
    private static readonly IDictionary<string, Type> _types = new Dictionary<string, Type>
    {
        { "gdrive", typeof(GDriveDatabaseBackupDestinationInformation) }
    };
    
    public static ICollection<IDatabaseBackupDestinationInformation> Create(IConfiguration configuration)
    {
        var result = new List<IDatabaseBackupDestinationInformation>();

        var sourceSections = configuration
            .GetSection("Destinations")
            .GetChildren();
        
        foreach (var source in sourceSections)
        {
            var key = source.GetValue<string>(nameof(IDatabaseBackupDestinationInformation.Type)) ??              
                      throw new ArgumentNullException(nameof(IDatabaseBackupSourceInformation.Type));
            
            var type = _types[key];
            var sourceInformation = (IDatabaseBackupDestinationInformation) Activator.CreateInstance(type)!;
            source.Bind(sourceInformation);
            result.Add(sourceInformation);
        }

        return result;
    }
}