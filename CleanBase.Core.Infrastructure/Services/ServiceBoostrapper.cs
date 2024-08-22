using CleanBase.Core.Infrastructure.Services.Caching;
using CleanBase.Core.Infrastructure.Services.Identity;
using CleanBase.Core.Infrastructure.Services.KeyVault;
using CleanBase.Core.Services.Core.Base;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.Services
{
    public static class ServiceBoostrapper
    {
        public static IServiceCollection UseMemoryCaching(this IServiceCollection services)
        {
            services.AddSingleton<ICacheManagerProvider, MemoryCacheManagerProvider>();
            return services;
        }

        public static IServiceCollection UseHttpIdentityProvider(this IServiceCollection services)
        {
            services.AddScoped<IIdentityProvider, HttpIdentityProvider>();
            return services;
        }

        public static IServiceCollection UseKeyVaultService(this IServiceCollection services)
        {
            services.AddTransient<IKeyVaultService, KeyVaultService>();
            return services;
        }
    }
}
