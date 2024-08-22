using Azure.Storage.Blobs;
using CleanBase.Core.Infrastructure.Storage.Azure;
using CleanBase.Core.Services.Storage;
using CleanBase.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.Storage
{
    public static class StorageBoostrapper
    {
        public static IServiceCollection UseAzureStorage(this IServiceCollection services, AppSettings appSettings)
        {
            services.AddSingleton<BlobServiceClient>(new BlobServiceClient(appSettings.AzureStorageSetting.BlobConnectionString));
            services.AddScoped<IStorageProvider, AzureStorageProvider>();
            return services;
        }
    }
}