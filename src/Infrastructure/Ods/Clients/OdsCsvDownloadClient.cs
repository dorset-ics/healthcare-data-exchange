using System.IO.Compression;
using Core.Ods.Abstractions;
using Core.Ods.Enums;
using Infrastructure.Ods.Configuration;

namespace Infrastructure.Ods.Clients;

public class OdsCsvDownloadClient(
    IHttpClientFactory clientFactory,
    OdsCsvDownloadConfiguration odsCsvDownloadConfiguration) : IOdsCsvDownloadClient
{
    public async Task<Stream> DownloadOrganisationsFromCsvSource(OdsCsvDownloadSource csvSource, CancellationToken cancellationToken)
    {
        bool downloadLocationFound = odsCsvDownloadConfiguration.DownloadLocations.TryGetValue(csvSource, out var downloadLocation);

        if (!downloadLocationFound)
        {
            throw new ArgumentException($"Download location for {csvSource} not found in configuration");
        }

        return await GetStreamAsync(downloadLocation!, cancellationToken);
    }

    private async Task<Stream> GetStreamAsync(string downloadLocation, CancellationToken cancellationToken)
    {
        var httpClient = clientFactory.CreateClient("OdsCsvDownloadHttpClient");
        var response = await httpClient.GetAsync(downloadLocation, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        bool isZipDownload = Path.GetExtension(downloadLocation) == ".zip";
        if (isZipDownload)
        {
            string fileName = Path.GetFileName(downloadLocation)!;
            string csvFileName = Path.ChangeExtension(fileName, ".csv")!;
            return ExtractCsvFromZip(responseStream, csvFileName, cancellationToken);
        }

        return responseStream;
    }

    private Stream ExtractCsvFromZip(Stream zipStream, string csvFileName, CancellationToken cancellationToken)
    {
        var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);

        var csvFileEntry = zipArchive.Entries.FirstOrDefault(entry => entry.Name == csvFileName);
        if (csvFileEntry == null)
        {
            throw new Exception($"CSV file {csvFileName} not found in zip archive");
        }

        return csvFileEntry.Open();
    }
}
