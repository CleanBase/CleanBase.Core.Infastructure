using CleanBase.Core.Data.Policies.Base;
using CleanBase.Core.Data.Policies.Generic;
using CleanBase.Core.Entities;
using CleanBase.Core.Entities.Base;
using CleanBase.Core.Services.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CleanBase.Core.Infrastructure.Policies
{
	public class PolicyFactory : IPolicyFactory
	{
		public IEnumerable<IDataPolicy> CreateCorePolicy(Type entityType, ICoreProvider coreProvider)
		{
			var dataPolicyList = new List<IDataPolicy>();
			dataPolicyList.AddRange(CreateCorePolicyInternal(entityType, coreProvider));
			dataPolicyList.AddRange(CreateChildCorePolicyInternal(entityType, coreProvider));
			return dataPolicyList;
		}

		protected virtual IEnumerable<IDataPolicy> CreateCorePolicyInternal(Type entityType, ICoreProvider coreProvider)
		{
			var policies = new List<IDataPolicy>();

			if (typeof(IActive).IsAssignableFrom(entityType))
			{
				policies.Add(CreatePolicy(typeof(ActiveRequiredPolicy<>), entityType));
			}
			if (typeof(IEntityAudit).IsAssignableFrom(entityType))
			{
				policies.Add(CreatePolicy(typeof(AuditRequiredPolicy<>), entityType, coreProvider));
			}
			if (typeof(IEntityName).IsAssignableFrom(entityType))
			{
				policies.Add(CreatePolicy(typeof(NameOrderByPolicy<>), entityType));
			}
			if (typeof(IOwnerEntity).IsAssignableFrom(entityType))
			{
				policies.Add(CreatePolicy(typeof(OwnerFilterPolicy<>), entityType, coreProvider));
			}
			if (typeof(IOwnerOrSystemEntity).IsAssignableFrom(entityType))
			{
				policies.Add(CreatePolicy(typeof(OwnerOrSystemFilterPolicy<>), entityType, coreProvider));
			}
			if (typeof(ITenanEntity).IsAssignableFrom(entityType))
			{
				policies.Add(CreatePolicy(typeof(TenantFilterPolicy<>), entityType, coreProvider));
				policies.Add(CreatePolicy(typeof(TenantEntityCreatingPolicy<>), entityType, coreProvider));
			}
			if (typeof(IAppEntity).IsAssignableFrom(entityType))
			{
				policies.Add(CreatePolicy(typeof(AppFilterPolicy<>), entityType, coreProvider));
				policies.Add(CreatePolicy(typeof(AppEntityCreatingPolicy<>), entityType, coreProvider));
			}

			return policies;
		}

		private IDataPolicy CreatePolicy(Type policyBaseType, Type entityType, ICoreProvider? coreProvider = null)
		{
			var policyType = policyBaseType.MakeGenericType(entityType);
			if (coreProvider == null)
			{
				return (IDataPolicy)Activator.CreateInstance(policyType)
					?? throw new InvalidOperationException($"Unable to create policy of type {policyType}");
			}
			return (IDataPolicy)Activator.CreateInstance(policyType, coreProvider)
				?? throw new InvalidOperationException($"Unable to create policy of type {policyType} with coreProvider");
		}

		protected virtual IEnumerable<IDataPolicy> CreateChildCorePolicyInternal(Type entityType, ICoreProvider coreProvider)
		{
			return Enumerable.Empty<IDataPolicy>();
		}
	}
}
