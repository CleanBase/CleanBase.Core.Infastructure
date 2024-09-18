using CleanBase.Core.Data.Policies.Base;
using CleanBase.Core.Data.Policies.Generic;
using CleanBase.Core.Data.Repositories;
using CleanBase.Core.Entities.Base;
using CleanBase.Core.Extensions;
using CleanBase.Core.Services.Core.Base;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.EF.Repositories
{
    public class EFRepositoryIdentity<T, E> : Repository<T>
        where T : class, IEntityCore
        where E : IdentityUser<Guid>
    {
        private DbSet<T> _set;

        protected DbSet<T> Set => _set ??= Context.Set<T>();

        protected readonly IdentityDbContext<E, IdentityRole<Guid>, Guid> Context;
        protected readonly ICoreProvider CoreProvider;

        public EFRepositoryIdentity(ICoreProvider coreProvider, IdentityDbContext<E, IdentityRole<Guid>, Guid> context)
        {
            this.Context = context;
            this.CoreProvider = coreProvider;
            this.AddCorePolicies();
        }

		protected virtual void AddCorePolicies()
		{
			this.AddPolicies(this.CoreProvider.PolicyFactory.CreateCorePolicy(this.EntityType, this.CoreProvider).ToArray<IDataPolicy>());
		}

		public override T Add(T entity, bool saveChanges = false)
        {
            entity.NormalizeData();
            this.ApplyAddPolicy(entity, true);
            if (this.Set.Local.Any(p => p == entity))
                return entity;
            var entityEntry = this.Set.Add(entity);
            if (saveChanges)
                this.Context.SaveChanges();
            return entityEntry.Entity;
        }

        public override async Task<T> AddAsync(T entity, bool saveChanges = false)
        {
            var addedEntity = this.Add(entity, false);
            if (saveChanges)
            {
                await this.Context.SaveChangesAsync();
            }
            return addedEntity;
        }

        protected virtual void ApplyAddPolicy(T entity, bool isBefore)
        {
            var addPolicies = this.Policies.OfType<IAddPolicy<T>>().ToList();
            if (!addPolicies.Any())
                return;

            foreach (var addPolicy in addPolicies)
            {
                if (isBefore)
                    addPolicy.ChallengeBeforeAdd(entity);
                else
                    addPolicy.ChallengeAfterAdd(entity);
            }
        }

        public override IQueryable<T> GetAll(bool ignoreGlobalFilter = false)
        {
            return !ignoreGlobalFilter ? this.ApplyFilterAndOrderPolicy(this.Set) : this.Set.IgnoreQueryFilters<T>();
        }

        private IQueryable<T> ApplyFilterAndOrderPolicy(IQueryable<T> query)
        {
            foreach (var policy in this.Policies)
            {
                if (policy is IFilterPolicy<T> filterPolicy)
                {
                    query = query.Where(filterPolicy.Predicate());
                }
                else if (policy is IOrderByPolicty<T> orderByPolicy)
                {
                    query = orderByPolicy.OrderBy(query);
                }
            }
            return query;
        }

        internal virtual IQueryable<T> GetAllOperation(IQueryable<T> query) => query;

        public override sealed async Task<IQueryable<T>> GetAllAsync(bool ignoreGlobalFilter = false)
        {
            return await Task.FromResult<IQueryable<T>>(this.GetAll(ignoreGlobalFilter));
        }

        public override IQueryable<T> GetAllInActive()
        {
            return this.ApplyFilterAndOrderPolicy(this.Set.IgnoreQueryFilters<T>());
        }

        public override sealed async Task<IQueryable<T>> GetAllInActiveAsync()
        {
            return await Task.FromResult<IQueryable<T>>(this.GetAllInActive());
        }

        protected virtual void ApplyFindPolicy(T entity)
        {
            if (entity == null)
                return;

            var findPolicies = this.Policies.OfType<IFindPolicy<T>>().ToList();
            if (!findPolicies.Any())
                return;

            foreach (var findPolicy in findPolicies)
            {
                findPolicy.ChallengeFind(entity);
            }
        }

        public override T Find(params object[] keys)
        {
            var entity = this.Set.Find(keys);
            this.ApplyFindPolicy(entity);
            return entity;
        }

        public override async Task<T> FindAsync(params object[] keys)
        {
            var entity = await this.Set.FindAsync(keys);
            this.ApplyFindPolicy(entity);
            return entity;
        }

        public override T FindInActive(params object[] keys)
        {
            var entity = this.Set.Find(keys);
            this.ApplyFindPolicy(entity);
            return entity;
        }

        public override async Task<T> FindInActiveAsync(params object[] keys)
        {
            var entity = await this.Set.FindAsync(keys);
            this.ApplyFindPolicy(entity);
            return entity;
        }

        public override IQueryable<T> Where(Expression<Func<T, bool>> predicate)
        {
            return this.ApplyFilterAndOrderPolicy(this.Set).Where(predicate);
        }

        public override async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            var entity = await this.Set.FirstOrDefaultAsync(predicate);
            this.ApplyFindPolicy(entity);
            return entity;
        }

        public override bool Delete(T entity, bool saveChanges = false)
        {
            if (entity == null)
                return false;

            this.ApplyDeletePolicy(entity);
            entity.MarkDeleted();
            if (saveChanges)
            {
                this.Context.SaveChanges();
            }
            return true;
        }

        public override bool Delete(params object[] keys) => this.Delete(this.Find(keys), false);

        public virtual bool Delete(string key) => this.Delete(new object[] { key });

        protected virtual void ApplyDeletePolicy(T entity)
        {
            if (entity == null)
                return;

            var deletePolicies = this.Policies.OfType<IDeletePolicy<T>>().ToList();
            if (!deletePolicies.Any())
                return;

            foreach (var deletePolicy in deletePolicies)
            {
                deletePolicy.ChallengeDelete(entity);
            }
        }

        protected virtual void ApplyUpdatePolicy(T entity, bool before)
        {
            if (entity == null)
                return;

            var updatePolicies = this.Policies.OfType<IUpdatePolicy<T>>().ToList();
            if (!updatePolicies.Any())
                return;

            foreach (var updatePolicy in updatePolicies)
            {
                if (before)
                    updatePolicy.ChallengeBeforeUpdate(entity);
                else
                    updatePolicy.ChallengeAfterUpdate(entity);
            }
        }

        public override T Update(T entity, bool saveChanges = false)
        {
            entity.NormalizeData();
            this.ApplyUpdatePolicy(entity, true);
            if (this.Set.Local.Any(p => p == entity))
                return entity;

            this.Set.Attach(entity);
            if (saveChanges)
            {
                this.Context.SaveChanges();
            }
            return entity;
        }


        public override void BatchAdd(IEnumerable<T> entities, bool saveChanges = false)
        {
            BulkAdd(entities, saveChanges);

            if (saveChanges)
            {
                this.Context.SaveChanges();
            }
        }

        public override async Task BatchAddAsync(IEnumerable<T> entities, bool saveChanges = false)
        {
            await BulkAddAsync(entities, saveChanges);
            if (saveChanges)
            {
                await this.Context.SaveChangesAsync();
            }
        }

        public override void BatchUpdate(IEnumerable<T> entities, bool saveChanges = false)
        {
            BulkUpdate(entities, saveChanges);

            if (saveChanges)
            {
                this.Context.SaveChanges();
            }
        }

        public override void BatchDelete(IEnumerable<T> entities, bool saveChanges = false)
        {
            BulkDelete(entities, saveChanges);

            if (saveChanges)
            {
                this.Context.SaveChanges();
            }
        }

        public override void BatchDelete(IEnumerable<string> keys, bool saveChanges = false)
        {
            var entities = keys.Select(k => this.Find(k)).Where(e => e != null).ToList();

            BulkDelete(entities, saveChanges);

            if (saveChanges)
            {
                this.Context.SaveChanges();
            }
        }

        public override bool HardDelete(T entity, bool saveChanges = false)
        {
            if (entity == null)
                return false;

            this.ApplyDeletePolicy(entity);
            this.Set.Remove(entity);
            if (saveChanges)
            {
                this.Context.SaveChanges();
            }
            return true;
        }

        public override bool HardDelete(params object[] keys) => this.HardDelete(this.Find(keys), false);

        public virtual bool HardDelete(string key) => this.HardDelete(new object[] { key });

        public override void BulkAdd(IEnumerable<T> entities, bool saveChanges = false)
        {
            foreach (var entity in entities)
            {
                entity.NormalizeData();
                this.ApplyAddPolicy(entity, true);
            }
            this.Context.BulkInsert(entities is IList<T> list ? list : entities.ToList());
            if (saveChanges)
            {
                this.Context.SaveChanges();
            }
        }

        public override async Task BulkAddAsync(IEnumerable<T> entities, bool saveChanges = false)
        {
            foreach (var entity in entities)
            {
                entity.NormalizeData();
                this.ApplyAddPolicy(entity, true);
            }
            await this.Context.BulkInsertAsync(entities is IList<T> list ? list : entities.ToList());
            if (saveChanges)
            {
                await this.Context.SaveChangesAsync();
            }
        }

        public override void BulkUpdate(IEnumerable<T> entities, bool saveChanges = false)
        {
            foreach (var entity in entities)
            {
                entity.NormalizeData();
                this.ApplyUpdatePolicy(entity, true);
            }
            this.Context.BulkUpdate(entities is IList<T> list ? list : entities.ToList());
            if (saveChanges)
            {
                this.Context.SaveChanges();
            }
        }

        public override async Task BulkUpdateAsync(IEnumerable<T> entities, bool saveChanges = false)
        {
            foreach (var entity in entities)
            {
                entity.NormalizeData();
                this.ApplyUpdatePolicy(entity, true);
            }
            await this.Context.BulkUpdateAsync(entities is IList<T> list ? list : entities.ToList());

            if (saveChanges)
            {
                await this.Context.SaveChangesAsync();
            }
        }

        public override void BulkDelete(IEnumerable<T> entities, bool saveChanges = false)
        {
            foreach (var entity in entities)
            {
                entity.NormalizeData();
                this.ApplyDeletePolicy(entity);
                entity.MarkDeleted();
            }

            this.Context.BulkUpdateAsync(entities is IList<T> list ? list : entities.ToList());

            if (saveChanges)
            {
                this.Context.SaveChangesAsync();
            }
        }

        public override void BulkDelete(IEnumerable<string> keys, bool saveChanges = false)
        {
            foreach (string key in keys)
                this.Delete(key);

            if (saveChanges)
            {
                this.Context.SaveChanges();
            }
        }

        public override async Task<bool> DeleteAsync(T entity, bool saveChanges = false)
        {
            if (entity == null)
                return await Task.FromResult(false);

            this.ApplyDeletePolicy(entity);
            entity.MarkDeleted();
            if (saveChanges)
            {
                await this.Context.SaveChangesAsync();
            }
            return await Task.FromResult(true);
        }

        public override async Task<bool> HardDeleteAsync(T entity, bool saveChanges = false)
        {
            if (entity == null)
                return await Task.FromResult(false);

            this.ApplyDeletePolicy(entity);
            this.Set.Remove(entity);
            if (saveChanges)
            {
                await this.Context.SaveChangesAsync();
            }
            return await Task.FromResult(true);
        }
        public override async Task<bool> DeleteAsync(params object[] keys) => await this.DeleteAsync(await this.FindAsync(keys), false);

        public override async Task<bool> HardDeleteAsync(params object[] keys) => await this.HardDeleteAsync(this.Find(keys), false);

        public override async Task<T> UpdateAsync(T entity, bool saveChanges = false)
        {
            entity.NormalizeData();
            this.ApplyUpdatePolicy(entity, true);
            if (this.Set.Local.Any(p => p == entity))
                return await Task.FromResult(entity);

            this.Set.Attach(entity);
            if (saveChanges)
            {
                await this.Context.SaveChangesAsync();
            }
            return entity;
        }

        public override async Task BatchUpdateAsync(IEnumerable<T> entities, bool saveChanges = false)
        {
            await BulkUpdateAsync(entities, saveChanges);

            if (saveChanges)
            {
                await this.Context.SaveChangesAsync();
            }
        }
    }
}
