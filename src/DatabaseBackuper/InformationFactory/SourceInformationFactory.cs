using DatabaseBackuper.Sources;
using DatabaseBackuper.Sources.Postgres;

namespace DatabaseBackuper.InformationFactory;

public static class SourceInformationFactory
{
    private static readonly IDictionary<string, Type> _types = new Dictionary<string, Type>
    {
        { "postgres", typeof(PostgresDatabaseBackupInformation) }
    };

    public static ICollection<IDatabaseBackupSourceInformation> Create(IConfiguration configuration)
    {
        var result = new List<IDatabaseBackupSourceInformation>();

        var sourceSections = configuration
            .GetSection("Sources")
            .GetChildren();

        foreach (var source in sourceSections)
        {
            var key = source.GetValue<string>(nameof(IDatabaseBackupSourceInformation.Type)) ??
                      throw new ArgumentNullException(nameof(IDatabaseBackupSourceInformation.Type));
            
            var type = _types[key];
            var sourceInformation = (IDatabaseBackupSourceInformation)Activator.CreateInstance(type)!;
            source.Bind(sourceInformation);
            result.Add(sourceInformation);
        }

        return result;
    }
}