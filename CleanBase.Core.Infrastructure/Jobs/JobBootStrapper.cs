using CleanBase.Core.Infrastructure.Jobs.Hangfire;
using CleanBase.Core.Infrastructure.Jobs.Hosting;
using CleanBase.Core.Services.Jobs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.Jobs
{
	public static class JobBootStrapper
	{
		public static IServiceCollection UseHangfireBackgroundJob(this IServiceCollection services)
		{
			services.AddHostedService<HostedService>();
			services.AddSingleton<IBackgroundJob, HangfireBackgroundJob>();
			return services;
		}
	}
}
