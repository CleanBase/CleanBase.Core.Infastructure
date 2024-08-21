using CleanBase.Core.Data.Transactions;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.EF.Transactions
{
	public class EFTransaction : ICustomTransaction, IDisposable
	{
		private IDbContextTransaction _dbContextTransaction;

		public EFTransaction(IDbContextTransaction dbContextTransaction) => _dbContextTransaction = dbContextTransaction;

		public void Commit() => this._dbContextTransaction.Commit();

		public void Dispose() => this._dbContextTransaction.Dispose();

		public void Rollback() => this._dbContextTransaction.Rollback();
	}
}
