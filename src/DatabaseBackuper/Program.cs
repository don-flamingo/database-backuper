using DatabaseBackuper;
using DatabaseBackuper.Destinations;
using DatabaseBackuper.Destinations.GDrive;
using DatabaseBackuper.Sources;
using DatabaseBackuper.Sources.Postgres;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services
            // sources
            .AddSingleton<IDatabaseBackupSource, PostgresDatabaseBackupSource>()
            // destinations
            .AddSingleton<IDatabaseBackupDestination, GDriveDatabaseBackupDestination>()
            .AddHostedService<Worker>();
    })
    .UseSerilog((hostingContext, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(hostingContext.Configuration)  
        .Enrich.FromLogContext()) 
    .Build();

host.Run();