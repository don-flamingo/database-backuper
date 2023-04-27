using System.Diagnostics.CodeAnalysis;
using Cronos;
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
    
    private System.Timers.Timer _timer;
    private readonly CronExpression _expression;
    private readonly TimeZoneInfo _timeZoneInfo;

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
        
        var cron = _configuration.GetValue<string>("Cron");
        _expression = CronExpression.Parse(cron);
        _timeZoneInfo = TimeZoneInfo.Local;
    }

    #region Cron
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ScheduleJob(stoppingToken);
    }

    protected virtual async Task ScheduleJob(CancellationToken cancellationToken)
    {
        var next = _expression.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);
        if (next.HasValue)
        {
            var delay = next.Value - DateTimeOffset.Now;
            if (delay.TotalMilliseconds <= 0)   // prevent non-positive values from being passed into Timer
            {
                await ScheduleJob(cancellationToken);
            }
            
            _timer = new System.Timers.Timer(delay.TotalMilliseconds);
            _timer.Elapsed += async (sender, args) =>
            {
                _timer.Dispose();  // reset and dispose timer
                _timer = null;

                if (!cancellationToken.IsCancellationRequested)
                {
                    await DoWork(cancellationToken);
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    await ScheduleJob(cancellationToken);    // reschedule next
                }
            };
            _timer.Start();
        }
        await Task.CompletedTask;
    }
    #endregion

    #region Logic
    public virtual async Task DoWork(CancellationToken cancellationToken)
    {
        var now = Now.ToUniversalTime();
        var datetime = now.ToString("yyyy_M_dd__HH_mm");
        var removeOlderThan = _configuration.GetValue<TimeSpan?>("RemoveBackupsOlderThan");
        
        foreach (var databaseBackupSourceInformation in _databaseBackupSourceInformation)
        {
            var filePath = await BackupAsync(databaseBackupSourceInformation, datetime, cancellationToken);

            await UploadAsync(databaseBackupSourceInformation, filePath, cancellationToken);
            
            File.Delete(filePath);
        }
        
        if (removeOlderThan.HasValue)
        {
            await DeleteOldBackups(now, removeOlderThan, cancellationToken);
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
    
    private async Task UploadAsync(
        IDatabaseBackupSourceInformation databaseBackupSourceInformation,
        string filePath,
        CancellationToken stoppingToken)
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
    
    private async Task DeleteOldBackups(DateTime now, [DisallowNull] TimeSpan? removeOlderThan,
        CancellationToken cancellationToken)
    {
        var removeOlderThanDateTime = now - removeOlderThan.Value;
        foreach (var databaseBackupDestinationInformation in _databaseBackupDestinationInformation)
        {
            var destination =
                _databaseBackupDestinations.FirstOrDefault(x => x.Type == databaseBackupDestinationInformation.Type)
                ?? throw new MissingDatabaseDestinationTypeException(databaseBackupDestinationInformation.Type);

            await destination.RemovePreviousBackupsAsync(removeOlderThanDateTime, databaseBackupDestinationInformation,
                cancellationToken);
        }
    }
    #endregion
}