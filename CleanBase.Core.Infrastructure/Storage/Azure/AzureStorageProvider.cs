using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using CleanBase.Core.Services.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.Storage.Azure
{
    public class AzureStorageProvider : IStorageProvider
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureStorageProvider(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<bool> CheckExistsAsync(string blobFileUrl)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(GetContainerNameFromAbsoluteUri(blobFileUrl));
            var blobClient = containerClient.GetBlobClient(GetRelatePath(blobFileUrl));
            return (await blobClient.ExistsAsync()).Value;
        }

        public Uri GenerateReadSasUri(string blobFileUrl, DateTimeOffset expiresOn)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(GetContainerNameFromAbsoluteUri(blobFileUrl));
            var blobClient = containerClient.GetBlobClient(GetRelatePath(blobFileUrl));

            var sasBuilder = new BlobSasBuilder(BlobSasPermissions.Read, expiresOn)
            {
                BlobContainerName = containerClient.Name,
                BlobName = blobClient.Name
            };

            return blobClient.GenerateSasUri(sasBuilder);
        }

        public async Task<Stream> ReadFileAsStreamAsync(string containerName, string relativePath)
        {
            var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(GetRelatePath(relativePath));
            var stream = new MemoryStream();
            await blobClient.DownloadToAsync(stream);
            stream.Position = 0;
            return stream;
        }

        public Task<Stream> ReadFileAsStreamAsync(string absoluteUri)
        {
            var containerName = GetContainerNameFromAbsoluteUri(absoluteUri);
            var relativePath = GetRelatePath(absoluteUri);
            return ReadFileAsStreamAsync(containerName, relativePath);
        }

        public async Task<string> ReadFileAsStringAsync(string containerName, string blobName)
        {
            using var contentStream = new MemoryStream();
            var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(GetRelatePath(blobName));
            await blobClient.DownloadToAsync(contentStream);
            contentStream.Position = 0;
            using var reader = new StreamReader(contentStream);
            return await reader.ReadToEndAsync();
        }

        public Task<string> ReadFileAsStringAsync(string absoluteUri)
        {
            var containerName = GetContainerNameFromAbsoluteUri(absoluteUri);
            var blobName = GetRelatePath(absoluteUri);
            return ReadFileAsStringAsync(containerName, blobName);
        }

        public async Task<string> ReadFileAsBase64Async(string containerName, string blobName)
        {
            using var contentStream = new MemoryStream();
            var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(GetRelatePath(blobName));
            await blobClient.DownloadToAsync(contentStream);
            return Convert.ToBase64String(contentStream.ToArray());
        }

        public Task<string> ReadFileAsBase64Async(string absoluteUri)
        {
            var containerName = GetContainerNameFromAbsoluteUri(absoluteUri);
            var blobName = GetRelatePath(absoluteUri);
            return ReadFileAsBase64Async(containerName, blobName);
        }

        public async Task<string> UploadBytesAsync(string relativeFilePath, string containerName, byte[] data, string? contentType = null)
        {
            var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(GetRelatePath(relativeFilePath));
            using var memoryStream = new MemoryStream(data);
            var httpHeaders = new BlobHttpHeaders { ContentType = contentType };
            var response = await blobClient.UploadAsync(memoryStream, httpHeaders);
            return blobClient.Uri.ToString();
        }

        public async Task<string> UploadFileAsync(string filePath, string blobFileUrl)
        {
            var containerName = GetContainerNameFromAbsoluteUri(blobFileUrl);
            var blobName = GetRelatePath(blobFileUrl);
            var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
            var response = await blobClient.UploadAsync(filePath);
            return blobClient.Uri.ToString();
        }

        private static string GetRelatePath(string blobName)
        {
            if (blobName.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(blobName, @"https://.*blob.core.windows.net/\w+/(.*)");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            return blobName;
        }

        private static string GetContainerNameFromAbsoluteUri(string absoluteUri)
        {
            if (absoluteUri.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(absoluteUri, @"https://.*blob.core.windows.net/(\w+)/");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            return string.Empty;
        }

        public async Task<IDictionary<string, string>> GetMetadataAsync(string blobContainerName, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
            var blobClient = containerClient.GetBlobClient(GetRelatePath(blobName));

            try
            {
                var properties = await blobClient.GetPropertiesAsync();
                return properties.Value.Metadata;
            }
            catch (RequestFailedException ex)
            {
                // Handle exception (log, rethrow, etc.)
                throw new Exception("Failed to get metadata", ex);
            }
        }

        public async Task SetMetadataAsync(string blobContainerName, string blobName, IDictionary<string, string> metadata)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
            var blobClient = containerClient.GetBlobClient(GetRelatePath(blobName));

            try
            {
                await blobClient.SetMetadataAsync(metadata);
            }
            catch (RequestFailedException ex)
            {
                // Handle exception (log, rethrow, etc.)
                throw new Exception("Failed to set metadata", ex);
            }
        }

        public async Task<IEnumerable<string>> ListFilesAsync(string blobContainerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);

            try
            {
                var blobItems = containerClient.GetBlobsAsync();
                var blobNames = new List<string>();

                await foreach (var blobItem in blobItems)
                {
                    blobNames.Add(blobItem.Name);
                }

                return blobNames;
            }
            catch (RequestFailedException ex)
            {
                // Handle exception (log, rethrow, etc.)
                throw new Exception("Failed to list files", ex);
            }
        }
    }
}
