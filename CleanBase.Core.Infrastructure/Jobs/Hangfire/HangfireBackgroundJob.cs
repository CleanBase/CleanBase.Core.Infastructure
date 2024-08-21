using CleanBase.Core.Services.Jobs;
using System;
using Hangfire;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.Jobs.Hangfire
{
	public class HangfireBackgroundJob : IBackgroundJob
	{
		private IBackgroundJobClient _client;

		public HangfireBackgroundJob(IBackgroundJobClient client) => _client = client;

		public string Enqueue(Exception<Action> action) 
		{
			return BackgroundJobClientExtensions.Enqueue(this._client,action);
		}
	}
}
