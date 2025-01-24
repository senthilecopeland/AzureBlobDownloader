using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Diagnostics.Metrics;
using System.IO;
using System.IO.Compression;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AzureBlobDownloader
{
    internal class App
    {
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient blobServiceClient;
        private readonly string containerName;
        private readonly string destinationFolder;
        private readonly DateTime searchFromDate;
        private readonly DateTime searchToDate;
        private readonly string[] filterWords;


        public App(IConfiguration configuration)
        {
            _configuration = configuration;

            var connectionString = _configuration["AppSettings:ConnectionString"];
            this.blobServiceClient = new BlobServiceClient(connectionString);

            this.containerName = _configuration["AppSettings:ContainerName"] ?? string.Empty;
            this.destinationFolder = Path.Combine(_configuration["AppSettings:DestinationFolder"] ?? Environment.CurrentDirectory,
                                     DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.fff"));

            this.searchFromDate = ParseDate(_configuration["FromDate"]);
            this.searchToDate = ParseDate(_configuration["ToDate"]);

            this.filterWords = _configuration["Filer"].Split(',');
        }

        public async Task Run()
        {
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            int counter = 0;

            await foreach (var blobItem in blobContainerClient.GetBlobsAsync())
            {
                if (blobItem.Properties.LastModified.HasValue)
                {
                    if (IsBlobMatchingFilter(blobItem))
                    {
                        Log.Information($"{++counter} - Fetching Matched Blob: {blobItem.Name}");

                        var download = await Download(blobContainerClient.GetBlobClient(blobItem.Name));
                        string content = UnZip(download);

                        var isMatched = SearchCriteriaMatched(content);

                        if (isMatched)
                        {
                            SavetoFile(content, blobItem.Name);
                            Log.Information($"Search Matched, Saving Blob");
                        }
                    }
                }
            }

            if (counter == 0)
            {
                Log.Information("No blobs found matching the search criteria.");
            }
        }

       private bool IsBlobMatchingFilter(BlobItem blob)
        {
            if (blob.Properties.LastModified.HasValue)
            {
                var lastModified = blob.Properties.LastModified.Value.DateTime.Date;

                if (lastModified >= this.searchFromDate && lastModified <= this.searchToDate)
                {
                    foreach (var filterWord in filterWords)
                    {
                        if (blob.Name.Contains(filterWord, StringComparison.OrdinalIgnoreCase)) 
                        {
                            return true; 
                        }
                    }

                }
            }
            return false;
        }

        private async Task<BlobDownloadInfo> Download(BlobClient blobClient)
        {
            return await blobClient.DownloadAsync();
        }

        private string UnZip(BlobDownloadInfo download)
        {
            using (var archive = new ZipArchive(download.Content, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    using (var entryStream = entry.Open())
                    {
                        using (var reader = new StreamReader(entryStream, Encoding.UTF8))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }

            return string.Empty;
        }

        private bool SearchCriteriaMatched(string content)
        {
            var search = _configuration["Search"] ?? string.Empty;
            return !string.IsNullOrWhiteSpace(search) && content.Contains(search, StringComparison.OrdinalIgnoreCase) ? true : false;
        }

        private void SavetoFile(string content, string blobName)
        {
            var folder = this.destinationFolder;
            if (blobName.Contains("/"))
            {
                var parts = blobName.Split("/");
                if (parts.Length > 1)
                {
                    blobName = parts[parts.Length - 1];
                    folder = Path.Combine(folder, parts[0]);
                }
            }

            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            var filePath = Path.Combine(folder, $"{blobName}.xml").Replace("/", "\\");

            File.WriteAllText(filePath, content);
        }

        private DateTime ParseDate(string dateStr) =>
            DateTime.TryParse(dateStr, out var parsedDate) ? parsedDate.Date : DateTime.Now.Date;


    }
}
