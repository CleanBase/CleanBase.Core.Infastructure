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
		protected virtual IEnumerable<IDataPolicy> CreateCorePolicyInternal(
		  Type entityType,
		  ICoreProvider coreProvider)
		{
			if (typeof(IActive).IsAssignableFrom(entityType))
				yield return Activator.CreateInstance(typeof(ActiveRequiredPolicy<>).MakeGenericType(entityType)) as IDataPolicy;
			if (typeof(IEntityAudit).IsAssignableFrom(entityType))
				yield return Activator.CreateInstance(typeof(AuditRequiredPolicy<>).MakeGenericType(entityType), (object)coreProvider) as IDataPolicy;
			if (typeof(IEntityName).IsAssignableFrom(entityType))
				yield return Activator.CreateInstance(typeof(NameOrderByPolicy<>).MakeGenericType(entityType)) as IDataPolicy;
			if (typeof(IOwnerEntity).IsAssignableFrom(entityType))
				yield return Activator.CreateInstance(typeof(OwnerFilterPolicy<>).MakeGenericType(entityType), (object)coreProvider) as IDataPolicy;
			if (typeof(IOwnerOrSystemEntity).IsAssignableFrom(entityType))
				yield return Activator.CreateInstance(typeof(OwnerOrSystemFilterPolicy<>).MakeGenericType(entityType), (object)coreProvider) as IDataPolicy;
			if (typeof(ITenanEntity).IsAssignableFrom(entityType))
			{
				yield return Activator.CreateInstance(typeof(TenantFilterPolicy<>).MakeGenericType(entityType), (object)coreProvider) as IDataPolicy;
				yield return Activator.CreateInstance(typeof(TenantEntityCreatingPolicy<>).MakeGenericType(entityType), (object)coreProvider) as IDataPolicy;
			}
			if (typeof(IAppEntity).IsAssignableFrom(entityType))
			{
				yield return Activator.CreateInstance(typeof(AppFilterPolicy<>).MakeGenericType(entityType), (object)coreProvider) as IDataPolicy;
				yield return Activator.CreateInstance(typeof(AppEntityCreatingPolicy<>).MakeGenericType(entityType), (object)coreProvider) as IDataPolicy;
			}
		}

		protected virtual IEnumerable<IDataPolicy> CreateChildCorePolicyInternal(
		  Type entityType,
		  ICoreProvider coreProvider)
		{
			return Enumerable.Empty<IDataPolicy>();
		}

		public IEnumerable<IDataPolicy> CreateCorePolicy(Type entityType, ICoreProvider coreProvider)
		{
			List<IDataPolicy> dataPolicyList = new List<IDataPolicy>();
			dataPolicyList.AddRange(this.CreateCorePolicyInternal(entityType, coreProvider));
			dataPolicyList.AddRange(this.CreateChildCorePolicyInternal(entityType, coreProvider));
			return (IEnumerable<IDataPolicy>)dataPolicyList;
		}
	}
}
