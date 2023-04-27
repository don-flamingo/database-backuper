FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY . ./

RUN dotnet restore src/DatabaseBackuper
RUN dotnet publish src/DatabaseBackuper -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:7.0  AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DatabaseBackuper.dll"]