using CleanBase.Core.Services.Jobs;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.Jobs.Hosting
{
	public class HostedService : IHostedService
	{
		IEnumerable<IProcessingJobConsumer> Jobs { get; set; }
		public HostedService(IEnumerable<IProcessingJobConsumer> jobs)
		{
			Jobs = jobs;
		}

		public virtual async Task StartAsync(CancellationToken cancellationToken)
		{
			foreach (IProcessingJobConsumer consumer in Jobs)
			{
				await consumer.StartAsync(cancellationToken);
			}
		}

		public virtual async Task StopAsync(CancellationToken cancellationToken)
		{
			foreach(IProcessingJobConsumer consumer in Jobs)
			{
				await consumer.StopAsync(cancellationToken);	
			}
		}
	}
}
