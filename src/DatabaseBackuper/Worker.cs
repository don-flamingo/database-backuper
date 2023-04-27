using DatabaseBackuper.Destinations;
using DatabaseBackuper.Exceptions;
using DatabaseBackuper.InformationFactory;
using DatabaseBackuper.Sources;
using static System.DateTime;

namespace DatabaseBackuper;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    
    private readonly ICollection<IDatabaseBackupSource> _databaseBackupSources;
    private readonly ICollection<IDatabaseBackupDestination> _databaseBackupDestinations;
    
    private readonly ICollection<IDatabaseBackupDestinationInformation> _databaseBackupDestinationInformation;
    private readonly ICollection<IDatabaseBackupSourceInformation> _databaseBackupSourceInformation;

    public Worker(
        ILogger<Worker> logger, 
        IConfiguration configuration, 
        IEnumerable<IDatabaseBackupSource> databaseBackupSources,
        IEnumerable<IDatabaseBackupDestination> databaseBackupDestinations)
    {
        _logger = logger;
        _configuration = configuration;
        
        _databaseBackupSources = databaseBackupSources.ToList();
        _databaseBackupDestinations = databaseBackupDestinations.ToList();

        _databaseBackupSourceInformation = SourceInformationFactory.Create(configuration);
        _databaseBackupDestinationInformation = DestinationInformationFactory.Create(configuration);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var datetime = Now.ToString("yyyy_M_dd__HH_mm");

        foreach (var databaseBackupSourceInformation in _databaseBackupSourceInformation)
        {
            var filePath = await BackupAsync(databaseBackupSourceInformation, datetime, stoppingToken);

            await UploadAsync(stoppingToken, databaseBackupSourceInformation, filePath);
            
            File.Delete(filePath);
        }
    }
    
    private async Task<string> BackupAsync(IDatabaseBackupSourceInformation databaseBackupSourceInformation, string datetime,
        CancellationToken stoppingToken)
    {
        var source = _databaseBackupSources.FirstOrDefault(x => x.Type == databaseBackupSourceInformation.Type)
                     ?? throw new MissingDatabaseSourceTypeException(databaseBackupSourceInformation.Type);

        var tmpDir = Path.Combine(".", "tmp");
        if (!Directory.Exists(tmpDir))
        {
            Directory.CreateDirectory(tmpDir);
        }

        var fileNameWithoutExtension = Path.Combine(tmpDir, $"{databaseBackupSourceInformation.Database}_{datetime}");
        var filePath = source.GetFileNameWithExtension(fileNameWithoutExtension);

        await source.BackupAsync(filePath, databaseBackupSourceInformation, stoppingToken);
        return filePath;
    }
    
    private async Task UploadAsync(CancellationToken stoppingToken,
        IDatabaseBackupSourceInformation databaseBackupSourceInformation, string filePath)
    {
        var destinationInformation =
            _databaseBackupDestinationInformation.FirstOrDefault(x =>
                x.Key == databaseBackupSourceInformation.DestinationKey)
            ?? throw new MissingDatabaseDestinationInformationException(databaseBackupSourceInformation.DestinationKey);

        var destination =
            _databaseBackupDestinations.FirstOrDefault(x => x.Type == destinationInformation.Type)
            ?? throw new MissingDatabaseDestinationTypeException(databaseBackupSourceInformation.Type);

        await destination.UploadBackupAsync(filePath, destinationInformation, stoppingToken);
    }

}