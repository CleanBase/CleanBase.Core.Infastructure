using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.Extensions
{
	public static class ServicesExtension
	{
		public static IServiceCollection RegisterDefaultInfrastructure(this IServiceCollection services)
		{
			return services;
		}
	}
}
