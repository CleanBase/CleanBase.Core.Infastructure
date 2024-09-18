using CleanBase.Core.Data.Repositories;
using CleanBase.Core.Data.Transactions;
using CleanBase.Core.Infrastructure.EF.Repositories;
using CleanBase.Core.Infrastructure.EF.Transactions;
using CleanBase.Core.Services.Core.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.EF.UnitOfWork
{
    public class EFUnitOfWorkIdentity<E> : CleanBase.Core.Data.UnitOfWorks.UnitOfWork  where E : IdentityUser<Guid>
    {
        private bool _disposed;
        private readonly IdentityDbContext<E, IdentityRole<Guid>, Guid> _context;
        private readonly Dictionary<Type, IRepository> _repository = new();
        protected readonly Dictionary<Type, Type> RepositoriesMapping = new();
        private readonly ICoreProvider _coreProvider;
        private ICustomTransaction _transaction;

        public EFUnitOfWorkIdentity(ICoreProvider coreProvider, IdentityDbContext<E, IdentityRole<Guid>, Guid> context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _coreProvider = coreProvider ?? throw new ArgumentNullException(nameof(coreProvider));
            InitRepositoriesMapping();
        }

        protected virtual void InitRepositoriesMapping() { }

        protected virtual Type? GetRepositoryTypeByEntityType<T>()
        {
            RepositoriesMapping.TryGetValue(typeof(T), out var repoType);
            return repoType;
        }

        public void AddRepository(IRepository repository)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            if (!_repository.ContainsKey(repository.EntityType))
                _repository.Add(repository.EntityType, repository);
        }

        public IdentityDbContext<E, IdentityRole<Guid>, Guid> GetContext() => _context;

        public override ICustomTransaction BeginTransaction()
        {
            if (_transaction != null)
                return new EmptyTransaction();

            _transaction = new EFTransaction(_context.Database.BeginTransaction());
            return _transaction;
        }

        public override void SaveChanges() => _context.SaveChanges();

        public override async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public override IRepository<T> GetRepositoryByEntityType<T>()
        {
            var repoType = GetRepositoryTypeByEntityType<T>();
            if (repoType != null)
                return (IRepository<T>)_coreProvider.ServiceProvider.GetService(repoType);

            if (!_repository.TryGetValue(typeof(T), out var repository))
                _repository[typeof(T)] = repository = new EFRepositoryIdentity<T,E>(_coreProvider, _context);

            return (IRepository<T>)repository;
        }

        public override TRepo Repository<TRepo>()
        {
            return _coreProvider.ServiceProvider.GetRequiredService<TRepo>();
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _context.Dispose();
            }

            _disposed = true;
        }
    }
}
