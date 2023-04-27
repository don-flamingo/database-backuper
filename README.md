# database-backuper

Trival .net worker to create regular database backups by cron schedule. After creation service will upload it on the secured storage (destination).  
Currently, worker can handle.

### Databases
- postgres

### Storages (Destinations)
- Google Drive

Want more? Do not be shy and contribute. Everything what You must code is implementation of the: 
- [IDatabaseBackupDestination](https://github.com/don-flamingo/database-backuper/blob/main/src/DatabaseBackuper.Destinations/IDatabaseBackupDestination.cs)
- [IDatabaseBackupDestinationInformation](https://github.com/don-flamingo/database-backuper/blob/main/src/DatabaseBackuper.Destinations/IDatabaseBackupDestinationInformation.cs)
- [IDatabaseBackupSource](https://github.com/don-flamingo/database-backuper/blob/main/src/DatabaseBackuper.Sources/IDatabaseBackupSource.cs)
- [IDatabaseBackupSourceInformation](https://github.com/don-flamingo/database-backuper/blob/main/src/DatabaseBackuper.Sources/IDatabaseBackupSourceInformation.cs)

## How to use this precious service? 

Fetch the REAL OG [DOCKER IMAGE](https://hub.docker.com/repository/docker/rogaliusz/database-backuper/general), then inject configuration.

```bash
docker run --detach \
    --restart=always \
    --name backuper \
    -v /usr/volumes/backuper:/app/credentials \
    -v /usr/volumes/backuper-tmp:/app/tmp \
    -e Cron="0 */6 * * *" \
    -e RemoveBackupsOlderThan="15.00:00:00" \
    -e Sources:0:Type="postgres" \
    -e Sources:0:Port="5432" \
    -e Sources:0:Host="192.168.0.1" \
    -e Sources:0:Database="my-awsome-db" \
    -e Sources:0:Username="postgres" \
    -e Sources:0:Type="postgres" \
    -e Sources:0:Password="my-awsome-pssw" \
    -e Sources:0:CustomArgs="--format=c" \
    -e Sources:0:DestinationKey="my-awseome-google-drive" \
    -e Destinations:0:Type="gdrive" \
    -e Destinations:0:Key="my-awseome-google-drive" \
    -e Destinations:0:DirectoryId="SOMEID" \
    -e Destinations:0:CredentialsPath="./credentials/google_credentials.json" \
    rogaliusz/database-backuper:7
```

Enjoy
