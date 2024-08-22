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
        private static readonly Dictionary<Type, Type> CorePolicyTypes = new()
        {
            { typeof(IActive), typeof(ActiveRequiredPolicy<>) },
            { typeof(IEntityAudit), typeof(AuditRequiredPolicy<>) },
            { typeof(IEntityName), typeof(NameOrderByPolicy<>) },
            { typeof(IOwnerEntity), typeof(OwnerFilterPolicy<>) },
            { typeof(IOwnerOrSystemEntity), typeof(OwnerOrSystemFilterPolicy<>) },
            { typeof(ITenanEntity), typeof(TenantFilterPolicy<>) },
            { typeof(IAppEntity), typeof(AppFilterPolicy<>) }
        };

        public IEnumerable<IDataPolicy> CreateCorePolicy(Type entityType, ICoreProvider coreProvider)
        {
            var dataPolicies = new List<IDataPolicy>();

            dataPolicies.AddRange(CreateCorePolicyInternal(entityType, coreProvider));
            dataPolicies.AddRange(CreateChildCorePolicyInternal(entityType, coreProvider));

            return dataPolicies;
        }

        public IEnumerable<IDataPolicy> CreateCorePolicyInternal(
            Type entityType,
            ICoreProvider coreProvider)
        {
            var policies = new List<IDataPolicy>();

            foreach (var policyType in CorePolicyTypes)
            {
                if (policyType.Key.IsAssignableFrom(entityType))
                {
                    var genericPolicyType = policyType.Value.MakeGenericType(entityType);
                    var policy = (IDataPolicy)Activator.CreateInstance(genericPolicyType, coreProvider)
                        ?? throw new InvalidOperationException($"Unable to create policy of type {genericPolicyType}");
                    policies.Add(policy);
                }
            }

            if (typeof(ITenanEntity).IsAssignableFrom(entityType))
            {
                policies.Add((IDataPolicy)Activator.CreateInstance(typeof(TenantFilterPolicy<>).MakeGenericType(entityType), coreProvider)
                    ?? throw new InvalidOperationException($"Unable to create policy of type {typeof(TenantFilterPolicy<>).MakeGenericType(entityType)}"));

                policies.Add((IDataPolicy)Activator.CreateInstance(typeof(TenantEntityCreatingPolicy<>).MakeGenericType(entityType), coreProvider)
                    ?? throw new InvalidOperationException($"Unable to create policy of type {typeof(TenantEntityCreatingPolicy<>).MakeGenericType(entityType)}"));
            }

            if (typeof(IAppEntity).IsAssignableFrom(entityType))
            {
                policies.Add((IDataPolicy)Activator.CreateInstance(typeof(AppFilterPolicy<>).MakeGenericType(entityType), coreProvider)
                    ?? throw new InvalidOperationException($"Unable to create policy of type {typeof(AppFilterPolicy<>).MakeGenericType(entityType)}"));

                policies.Add((IDataPolicy)Activator.CreateInstance(typeof(AppEntityCreatingPolicy<>).MakeGenericType(entityType), coreProvider)
                    ?? throw new InvalidOperationException($"Unable to create policy of type {typeof(AppEntityCreatingPolicy<>).MakeGenericType(entityType)}"));
            }

            return policies;
        }

        protected virtual IEnumerable<IDataPolicy> CreateChildCorePolicyInternal(
            Type entityType,
            ICoreProvider coreProvider)
        {
            return Enumerable.Empty<IDataPolicy>();
        }
    }
}
